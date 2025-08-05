using Easy.Notifications.Core.Enums;
using Easy.Notifications.Core.Interfaces;
using Easy.Notifications.Providers.Email;
using Easy.Notifications.Providers.SignalR;
using Easy.Notifications.Providers.Slack;
using Easy.Notifications.Providers.Sms;
using Easy.Notifications.Providers.Teams;
using Easy.Notifications.Providers.Telegram;
using Easy.Notifications.Providers.WhatsApp;
using Easy.Notifications.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SendGrid;
using Twilio.Clients;

namespace Easy.Notifications.Configuration
{
    public static class NotificationBuilder
    {
        public static IServiceCollection AddEasyNotifications(this IServiceCollection services, IConfiguration configuration)
        {
            var section = configuration.GetSection("NotificationConfiguration");
            if (!section.Exists())
                throw new InvalidOperationException("Notification configuration section is missing.");

            var config = section.Get<NotificationConfiguration>();
            if (config == null)
                throw new InvalidOperationException("Notification configuration could not be bound.");

            services.AddHttpClient();

            #region Email

            if (config.NotificationOptions.UseEmail)
            {
                switch (config.NotificationProviders.EmailProvider)
                {
                    case EmailProviderType.Smtp:
                        EnsureSmtpConfig(config.EmailConfiguration);
                        services.Configure<EmailConfiguration>(section.GetSection("EmailConfiguration"));
                        services.AddSingleton<INotificationProvider, SmtpEmailNotificationProvider>();
                        break;
                    case EmailProviderType.Sendgrid:
                        EnsureSendGridConfig(config.EmailConfiguration);
                        services.Configure<EmailConfiguration>(section.GetSection("EmailConfiguration"));
                        services.AddSingleton<ISendGridClient>(sp =>
                        {
                            return new SendGridClient(config.EmailConfiguration?.ApiKey);
                        });
                        services.AddSingleton<INotificationProvider, SendgridEmailNotificationProvider>();
                        break;
                    case EmailProviderType.Mailgun:
                        EnsureMailgunConfig(config.EmailConfiguration);
                        services.Configure<EmailConfiguration>(section.GetSection("EmailConfiguration"));
                        services.AddSingleton<INotificationProvider, MailgunEmailNotificationProvider>();
                        break;
                    default:
                        throw new InvalidOperationException("Unsupported email provider.");
                }
            }

            #endregion

            #region SMS


            if (config.NotificationOptions.UseSms)
            {
                switch (config.NotificationProviders.SmsProvider)
                {
                    case SmsProviderType.Vonage: //nexmo
                        EnsureVonageConfig(config.SmsConfiguration);
                        services.Configure<SmsConfiguration>(section.GetSection("SmsConfiguration"));
                        services.AddSingleton<INotificationProvider, VonageSmsProvider>();
                        break;
                    case SmsProviderType.Twilio:
                        EnsureTwilioConfig(config.SmsConfiguration);
                        services.Configure<SmsConfiguration>(section.GetSection("SmsConfiguration"));
                        services.AddSingleton<ITwilioRestClient>(sp => new TwilioRestClient(config.SmsConfiguration.Username, config.SmsConfiguration.Password));
                        services.AddSingleton<INotificationProvider, TwilioSmsProvider>();
                        break;
                    default:
                        throw new InvalidOperationException("Unsupported SMS provider.");
                }

            }

            #endregion

            #region WhatsApp (Twilio)

            if (config.NotificationOptions.UseWhatsApp)
            {
                services.Configure<WhatsAppConfiguration>(section.GetSection("WhatsAppConfiguration"));
                services.AddSingleton<INotificationProvider, TwilioWhatsAppNotificationProvider>();
            }

            #endregion

            #region Telegram

            if (config.NotificationOptions.UseTelegram)
            {
                services.Configure<TelegramConfiguration>(section.GetSection("TelegramConfiguration"));
                services.AddHttpClient<TelegramNotificationProvider>();
                services.AddSingleton<INotificationProvider, TelegramNotificationProvider>();
            }

            #endregion

            #region Slack
            if (config.NotificationOptions.UseSlack)
            {
                EnsureSlackConfig(config.SlackConfiguration);

                services.Configure<SlackConfiguration>(section.GetSection("SlackConfiguration"));
                services.AddHttpClient<SlackNotificationProvider>();
                services.AddSingleton<INotificationProvider, SlackNotificationProvider>();
            }
            #endregion

            #region Teams
            if (config.NotificationOptions.UseTeams)
            {
                EnsureTeamsConfig(config.TeamsConfiguration);

                services.Configure<TeamsConfiguration>(section.GetSection("TeamsConfiguration"));
                services.AddHttpClient<TeamsNotificationProvider>();
                services.AddSingleton<INotificationProvider, TeamsNotificationProvider>();
            }
            #endregion

            #region SignalR

            if (config.NotificationOptions.UseSignalR)
            {

                services.AddSignalR();

                services.AddSingleton<INotificationProvider, SignalRNotificationProvider>();
            }

            #endregion

            services.AddSingleton<INotificationService, NotificationService>();

            return services;
        }


        #region Validation Helpers

        private static void EnsureSmtpConfig(EmailConfiguration config)
        {
            if (config == null)
                throw new InvalidOperationException("SMTP configuration section is missing.");

            if (string.IsNullOrWhiteSpace(config.Host) ||
                config.Port == 0 ||
                string.IsNullOrWhiteSpace(config.Sender) ||
                string.IsNullOrWhiteSpace(config.Username) ||
                string.IsNullOrWhiteSpace(config.Password))
                throw new InvalidOperationException("SMTP email configuration is incomplete.");
        }

        private static void EnsureSendGridConfig(EmailConfiguration config)
        {
            if (config == null)
                throw new InvalidOperationException("SendGrid configuration section is missing.");

            if (string.IsNullOrWhiteSpace(config.ApiKey) ||
                string.IsNullOrWhiteSpace(config.Sender) ||
                string.IsNullOrWhiteSpace(config.SenderDisplayName))
                throw new InvalidOperationException("SendGrid email configuration is incomplete.");
        }

        private static void EnsureMailgunConfig(EmailConfiguration config)
        {

            if (config == null)
                throw new InvalidOperationException("Mailgun configuration section is missing.");

            if (string.IsNullOrWhiteSpace(config.ApiKey) ||
                string.IsNullOrWhiteSpace(config.Domain))
                throw new InvalidOperationException("Mailgun configuration is incomplete.");
        }

        private static void EnsureVonageConfig(SmsConfiguration config)
        {

            if (config == null)
                throw new InvalidOperationException("Vonage SMS configuration section is missing.");

            if (string.IsNullOrWhiteSpace(config.Username) ||
                string.IsNullOrWhiteSpace(config.Password) ||
                string.IsNullOrWhiteSpace(config.Sender))
                throw new InvalidOperationException("Vonage SMS configuration is incomplete.");
        }

        private static void EnsureTwilioConfig(SmsConfiguration config)
        {
            if (config == null)
                throw new InvalidOperationException("Twilio SMS configuration section is missing.");


            if (string.IsNullOrWhiteSpace(config.Username) ||
                string.IsNullOrWhiteSpace(config.Password) ||
                string.IsNullOrWhiteSpace(config.Sender))
                throw new InvalidOperationException("Twilio SMS configuration is incomplete.");
        }

        private static void EnsureWhatsAppConfig(WhatsAppConfiguration config)
        {
            if (config == null)
                throw new InvalidOperationException("WhatsApp configuration section is missing.");

            if (string.IsNullOrWhiteSpace(config.AccountSid) ||
                string.IsNullOrWhiteSpace(config.AuthToken) ||
                string.IsNullOrWhiteSpace(config.Sender))
                throw new InvalidOperationException("WhatsApp configuration is incomplete. Required fields: AccountSid, AuthToken, Sender.");
        }

        private static void EnsureTelegramConfig(TelegramConfiguration config)
        {
            if (config == null)
                throw new InvalidOperationException("Telegram configuration section is missing.");

            if (string.IsNullOrWhiteSpace(config.BotToken) ||
                config.ChatId == null ||
               string.IsNullOrEmpty(config.ChatId))
                throw new InvalidOperationException("Telegram configuration is incomplete. Required fields: BotToken, ChatId.");
        }

        private static void EnsureSlackConfig(SlackConfiguration config)
        {
            if (config == null)
                throw new InvalidOperationException("Slack configuration section is missing.");

            if (string.IsNullOrWhiteSpace(config.WebhookUrl))
                throw new InvalidOperationException("Slack WebhookUrl is required.");
        }

        private static void EnsureTeamsConfig(TeamsConfiguration config)
        {
            if (config == null)
                throw new InvalidOperationException("Teams configuration section is missing.");

            if (string.IsNullOrWhiteSpace(config.WebhookUrl))
                throw new InvalidOperationException("Teams WebhookUrl is required.");
        }

        #endregion


    }
}
