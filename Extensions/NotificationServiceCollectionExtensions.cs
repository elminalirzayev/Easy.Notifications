using Easy.Notifications.Core.Abstractions;
using Easy.Notifications.Core.Models;
using Easy.Notifications.Infrastructure.Dispatcher;
using Easy.Notifications.Infrastructure.Templating;
using Easy.Notifications.Providers.Chat;
using Easy.Notifications.Providers.Email;
using Easy.Notifications.Providers.Realtime;
using Easy.Notifications.Providers.Sms;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Threading.Channels;
using Twilio.Clients;

namespace Easy.Notifications.Extensions
{
    /// <summary>
    /// Extension methods for configuring Easy.Notifications services and providers.
    /// </summary>
    public static class NotificationServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the core notification services including Dispatcher, In-memory Queue, and Template Engine.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <returns>The updated service collection.</returns>
        public static IServiceCollection AddEasyNotifications(this IServiceCollection services)
        {
            services.TryAddSingleton<ITemplateEngine, StringTemplateEngine>();
            services.TryAddSingleton<INotificationService, NotificationDispatcher>();

            // Create a thread-safe unbounded channel for background processing
            services.AddSingleton(Channel.CreateUnbounded<NotificationPayload>());

            // Register the background worker that processes the queue
            services.AddHostedService<BackgroundNotificationWorker>();

            return services;
        }

        /// <summary>
        /// Registers the SMTP Email provider.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="config">Application configuration.</param>
        /// <returns>The updated service collection.</returns>
        public static IServiceCollection AddSmtpEmail(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<EmailConfiguration>(config.GetSection("NotificationConfiguration:EmailConfiguration"));
            services.TryAddEnumerable(ServiceDescriptor.Transient<INotificationProvider, SmtpEmailProvider>());
            return services;
        }

        /// <summary>
        /// Registers the SendGrid Email provider.
        /// </summary>
        public static IServiceCollection AddSendGrid(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<SendGridConfiguration>(config.GetSection("NotificationConfiguration:SendGridConfiguration"));

            // Register SendGridClient using the API key from configuration
            services.AddSingleton<SendGrid.ISendGridClient>(sp =>
                new SendGrid.SendGridClient(config["NotificationConfiguration:SendGridConfiguration:ApiKey"]));

            services.TryAddEnumerable(ServiceDescriptor.Transient<INotificationProvider, SendGridProvider>());
            return services;
        }

        /// <summary>
        /// Registers the Mailgun Email provider and its HttpClient.
        /// </summary>
        public static IServiceCollection AddMailgun(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<MailgunConfiguration>(config.GetSection("NotificationConfiguration:MailgunConfiguration"));
            services.AddHttpClient("Mailgun");
            services.TryAddEnumerable(ServiceDescriptor.Transient<INotificationProvider, MailgunProvider>());
            return services;
        }

        /// <summary>
        /// Registers Twilio providers for both SMS and WhatsApp.
        /// </summary>
        public static IServiceCollection AddTwilio(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<TwilioConfiguration>(config.GetSection("NotificationConfiguration:TwilioConfiguration"));

            // Register ITwilioRestClient for dependency injection in Twilio providers
            services.AddSingleton<ITwilioRestClient>(sp =>
            {
                var cfg = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<TwilioConfiguration>>().Value;
                return new TwilioRestClient(cfg.AccountSid, cfg.AuthToken);
            });

            services.TryAddEnumerable(ServiceDescriptor.Transient<INotificationProvider, TwilioSmsProvider>());
            services.TryAddEnumerable(ServiceDescriptor.Transient<INotificationProvider, TwilioWhatsAppProvider>());
            return services;
        }

        /// <summary>
        /// Registers Vonage providers for both SMS and WhatsApp.
        /// </summary>
        public static IServiceCollection AddVonage(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<VonageConfiguration>(config.GetSection("NotificationConfiguration:VonageConfiguration"));
            services.AddHttpClient("Vonage");

            services.TryAddEnumerable(ServiceDescriptor.Transient<INotificationProvider, VonageSmsProvider>());
            services.TryAddEnumerable(ServiceDescriptor.Transient<INotificationProvider, VonageWhatsAppProvider>());
            return services;
        }

        /// <summary>
        /// Registers Chat providers including Slack, Microsoft Teams, and Telegram.
        /// </summary>
        public static IServiceCollection AddChatProviders(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<TelegramConfiguration>(config.GetSection("NotificationConfiguration:TelegramConfiguration"));

            // Register named HttpClients for each provider to manage lifetimes properly
            services.AddHttpClient("Slack");
            services.AddHttpClient("Teams");
            services.AddHttpClient("Telegram");

            services.TryAddEnumerable(ServiceDescriptor.Transient<INotificationProvider, SlackProvider>());
            services.TryAddEnumerable(ServiceDescriptor.Transient<INotificationProvider, TeamsProvider>());
            services.TryAddEnumerable(ServiceDescriptor.Transient<INotificationProvider, TelegramProvider>());

            return services;
        }

        /// <summary>
        /// Registers SignalR real-time notification provider.
        /// </summary>
        public static IServiceCollection AddSignalRNotifications(this IServiceCollection services)
        {
            // Note: SignalR Hub must be mapped in the Middleware (app.MapHub)
            services.AddSignalR();
            services.TryAddEnumerable(ServiceDescriptor.Transient<INotificationProvider, SignalRProvider>());
            return services;
        }
    }
}