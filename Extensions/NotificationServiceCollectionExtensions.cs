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
    /// Extension methods for configuring Easy.Notifications.
    /// </summary>
    public static class NotificationServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the core notification services (Dispatcher, Queue, Template Engine).
        /// </summary>
        public static IServiceCollection AddEasyNotifications(this IServiceCollection services)
        {
            services.TryAddSingleton<ITemplateEngine, StringTemplateEngine>();
            services.TryAddSingleton<INotificationService, NotificationDispatcher>();
            services.AddSingleton(Channel.CreateUnbounded<NotificationPayload>());
            services.AddHostedService<BackgroundNotificationWorker>();
            return services;
        }

        public static IServiceCollection AddSmtpEmail(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<EmailConfiguration>(config.GetSection("NotificationConfiguration:EmailConfiguration"));
            services.TryAddEnumerable(ServiceDescriptor.Singleton<INotificationProvider, SmtpEmailProvider>());
            return services;
        }

        public static IServiceCollection AddSendGrid(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<SendGridConfiguration>(config.GetSection("NotificationConfiguration:SendGridConfiguration"));
            services.AddSingleton<SendGrid.ISendGridClient>(sp => new SendGrid.SendGridClient(config["SendGridConfiguration:ApiKey"]));
            services.TryAddEnumerable(ServiceDescriptor.Singleton<INotificationProvider, SendGridProvider>());
            return services;
        }

        public static IServiceCollection AddMailgun(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<MailgunConfiguration>(config.GetSection("NotificationConfiguration:MailgunConfiguration"));
            services.AddHttpClient("Mailgun");
            services.TryAddEnumerable(ServiceDescriptor.Singleton<INotificationProvider, MailgunProvider>());
            return services;
        }

        public static IServiceCollection AddTwilio(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<TwilioConfiguration>(config.GetSection("NotificationConfiguration:TwilioConfiguration"));
            services.AddSingleton<ITwilioRestClient>(sp =>
            {
                var cfg = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<TwilioConfiguration>>().Value;
                return new TwilioRestClient(cfg.AccountSid, cfg.AuthToken);
            });

            // Registers both SMS and WhatsApp
            services.TryAddEnumerable(ServiceDescriptor.Singleton<INotificationProvider, TwilioSmsProvider>());
            services.TryAddEnumerable(ServiceDescriptor.Singleton<INotificationProvider, TwilioWhatsAppProvider>());
            return services;
        }

        public static IServiceCollection AddVonage(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<VonageConfiguration>(config.GetSection("NotificationConfiguration:VonageConfiguration"));
            services.AddHttpClient("Vonage");
            services.TryAddEnumerable(ServiceDescriptor.Singleton<INotificationProvider, VonageSmsProvider>());
            return services;
        }

        public static IServiceCollection AddChatProviders(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<TelegramConfiguration>(config.GetSection("NotificationConfiguration:TelegramConfiguration"));

            services.AddHttpClient("Slack");
            services.AddHttpClient("Teams");
            services.AddHttpClient("Telegram");

            services.TryAddEnumerable(ServiceDescriptor.Singleton<INotificationProvider, SlackProvider>());
            services.TryAddEnumerable(ServiceDescriptor.Singleton<INotificationProvider, TeamsProvider>());
            services.TryAddEnumerable(ServiceDescriptor.Singleton<INotificationProvider, TelegramProvider>());
            return services;
        }

        public static IServiceCollection AddSignalRNotifications(this IServiceCollection services)
        {
            services.AddSignalR();
            services.TryAddEnumerable(ServiceDescriptor.Singleton<INotificationProvider, SignalRProvider>());
            return services;
        }
    }
}