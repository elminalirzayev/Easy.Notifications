using System.Text.Json.Serialization;

namespace Easy.Notifications.Providers.Slack.Models
{
    /// <summary>
    /// Represents the root payload sent to a Slack Webhook.
    /// </summary>
    public class SlackPayload
    {
        /// <summary>
        /// Gets or sets the fallback text (shown in popups or if blocks fail).
        /// </summary>
        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the visual blocks (Block Kit).
        /// </summary>
        [JsonPropertyName("blocks")]
        public List<SlackBlock> Blocks { get; set; } = new();
    }

    /// <summary>
    /// Represents a structural block in a Slack message (e.g., Header, Section).
    /// </summary>
    public class SlackBlock
    {
        /// <summary>
        /// Gets or sets the type of block (e.g., "header", "section", "divider").
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the text object within the block (if applicable).
        /// </summary>
        [JsonPropertyName("text")]
        public SlackText? Text { get; set; }

        /// <summary>
        /// Gets or sets additional accessory elements (e.g., images, buttons) - Optional.
        /// </summary>
        [JsonPropertyName("elements")]
        public List<SlackText>? Elements { get; set; }
        // ---------------------------

        [JsonPropertyName("accessory")]
        public SlackElement? Accessory { get; set; }
    }


    /// <summary>
    /// Represents a text object within a block.
    /// </summary>
    public class SlackText
    {
        /// <summary>
        /// Gets or sets the text type. Usually "mrkdwn" or "plain_text".
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = "mrkdwn";

        /// <summary>
        /// Gets or sets the actual text content.
        /// </summary>
        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;

        public SlackText(string text, string type = "mrkdwn")
        {
            Text = text;
            Type = type;
        }
    }

    /// <summary>
    /// Represents an interactive or visual element (e.g., Image).
    /// </summary>
    public class SlackElement
    {
        /// <summary>
        /// Gets or sets the element type (e.g., "image").
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the image URL.
        /// </summary>
        [JsonPropertyName("image_url")]
        public string? ImageUrl { get; set; }

        /// <summary>
        /// Gets or sets the alt text for the image.
        /// </summary>
        [JsonPropertyName("alt_text")]
        public string? AltText { get; set; }
    }
}