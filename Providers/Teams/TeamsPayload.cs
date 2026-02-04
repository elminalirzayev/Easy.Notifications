using System.Text.Json.Serialization;

namespace Easy.Notifications.Providers.Teams.Models
{
    /// <summary>
    /// Represents the legacy MessageCard format for Microsoft Teams Incoming Webhooks.
    /// </summary>
    public class TeamsMessageCard
    {
        /// <summary>
        /// Gets or sets the type of the card. Must be "MessageCard".
        /// </summary>
        [JsonPropertyName("@type")]
        public string Type { get; set; } = "MessageCard";

        /// <summary>
        /// Gets or sets the context. Must be "https://schema.org/extensions".
        /// </summary>
        [JsonPropertyName("@context")]
        public string Context { get; set; } = "https://schema.org/extensions";

        /// <summary>
        /// Gets or sets the summary line (shown in notification popups).
        /// </summary>
        [JsonPropertyName("summary")]
        public string? Summary { get; set; }

        /// <summary>
        /// Gets or sets the theme color (Hex code without #). Example: "0078D7".
        /// </summary>
        [JsonPropertyName("themeColor")]
        public string? ThemeColor { get; set; }

        /// <summary>
        /// Gets or sets the title of the card.
        /// </summary>
        [JsonPropertyName("title")]
        public string? Title { get; set; }

        /// <summary>
        /// Gets or sets the main text content.
        /// </summary>
        [JsonPropertyName("text")]
        public string? Text { get; set; }

        /// <summary>
        /// Gets or sets the list of sections.
        /// </summary>
        [JsonPropertyName("sections")]
        public List<TeamsSection> Sections { get; set; } = new();

        /// <summary>
        /// Gets or sets the list of potential actions (buttons).
        /// </summary>
        [JsonPropertyName("potentialAction")]
        public List<TeamsPotentialAction>? PotentialAction { get; set; }
    }

    /// <summary>
    /// Represents a section within the Teams card.
    /// </summary>
    public class TeamsSection
    {
        /// <summary>
        /// Gets or sets the title of the activity.
        /// </summary>
        [JsonPropertyName("activityTitle")]
        public string? ActivityTitle { get; set; }

        /// <summary>
        /// Gets or sets the subtitle of the activity.
        /// </summary>
        [JsonPropertyName("activitySubtitle")]
        public string? ActivitySubtitle { get; set; }

        /// <summary>
        /// Gets or sets the image URL for the activity.
        /// </summary>
        [JsonPropertyName("activityImage")]
        public string? ActivityImage { get; set; }

        /// <summary>
        /// Gets or sets the text content of the section.
        /// </summary>
        [JsonPropertyName("text")]
        public string? Text { get; set; }

        /// <summary>
        /// Gets or sets a list of facts (key-value pairs).
        /// </summary>
        [JsonPropertyName("facts")]
        public List<TeamsFact> Facts { get; set; } = new();

        /// <summary>
        /// Gets or sets a value indicating whether markdown is enabled.
        /// </summary>
        [JsonPropertyName("markdown")]
        public bool Markdown { get; set; } = true;
    }

    /// <summary>
    /// Represents a Key-Value fact in a section.
    /// </summary>
    public class TeamsFact
    {
        /// <summary>
        /// Gets or sets the name of the fact.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the value of the fact.
        /// </summary>
        [JsonPropertyName("value")]
        public string Value { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TeamsFact"/> class.
        /// </summary>
        /// <param name="name">Fact name.</param>
        /// <param name="value">Fact value.</param>
        public TeamsFact(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }

    /// <summary>
    /// Represents an action (button) on the card.
    /// </summary>
    public class TeamsPotentialAction
    {
        /// <summary>
        /// Gets or sets the type of action. Default is "OpenUri".
        /// </summary>
        [JsonPropertyName("@type")]
        public string Type { get; set; } = "OpenUri";

        /// <summary>
        /// Gets or sets the name (label) of the button.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the targets (URIs) for the action.
        /// </summary>
        [JsonPropertyName("targets")]
        public List<TeamsTarget> Targets { get; set; } = new();
    }

    /// <summary>
    /// Represents the target URI for an action.
    /// </summary>
    public class TeamsTarget
    {
        /// <summary>
        /// Gets or sets the OS type (default or specific).
        /// </summary>
        [JsonPropertyName("os")]
        public string Os { get; set; } = "default";

        /// <summary>
        /// Gets or sets the URI to open.
        /// </summary>
        [JsonPropertyName("uri")]
        public string Uri { get; set; } = string.Empty;
    }
}