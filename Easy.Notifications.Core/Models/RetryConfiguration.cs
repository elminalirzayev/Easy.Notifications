namespace Easy.Notifications.Core.Models
{
    public class RetryConfiguration
    {
        /// <summary>
        /// Maximum number of retry attempts before marking the notification as hard-failed.
        /// Default is 3.
        /// </summary>
        public int MaxRetryCount { get; set; } = 3;

        /// <summary>
        /// The interval in minutes between retry worker executions.
        /// Default is 5 minutes.
        /// </summary>
        public int IntervalInMinutes { get; set; } = 5;
    }
}
