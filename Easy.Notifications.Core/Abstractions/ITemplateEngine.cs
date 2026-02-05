namespace Easy.Notifications.Core.Abstractions
{
    /// <summary>
    /// Contract for a simple string replacement engine.
    /// </summary>
    public interface ITemplateEngine
    {
        /// <summary>
        /// Replaces placeholders in the content with values from the dictionary.
        /// </summary>
        /// <param name="content">The raw string with {{Keys}}.</param>
        /// <param name="data">The Key-Value pairs.</param>
        /// <returns>The processed string.</returns>
        string Process(string content, Dictionary<string, string> data);
    }
}