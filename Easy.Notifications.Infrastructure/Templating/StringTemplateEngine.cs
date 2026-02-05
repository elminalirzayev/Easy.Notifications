using Easy.Notifications.Core.Abstractions;
using System.Text.RegularExpressions;

namespace Easy.Notifications.Infrastructure.Templating
{
    /// <summary>
    /// A Regex-based template engine implementation.
    /// </summary>
    public class StringTemplateEngine : ITemplateEngine
    {
        /// <inheritdoc />
        public string Process(string content, Dictionary<string, string> data)
        {
            if (string.IsNullOrEmpty(content) || data == null || data.Count == 0)
                return content;

            // Replaces {{Key}} with Value (Case Insensitive)
            return Regex.Replace(content, @"\{\{(.+?)\}\}", match =>
            {
                var key = match.Groups[1].Value.Trim();
                return data.TryGetValue(key, out var value) ? value : match.Value;
            }, RegexOptions.IgnoreCase);
        }
    }
}