namespace Easy.Notifications.Providers.Teams
{
    public class TeamsMessageCard
    {
        public string Type { get; set; } = "MessageCard";
        public string Context { get; set; } = "https://schema.org/extensions";
        public string Summary { get; set; } = string.Empty;
        public string ThemeColor { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public List<TeamsSection> Sections { get; set; } = new();
        public List<TeamsPotentialAction> PotentialAction { get; set; } = new();
    }

    public class TeamsSection
    {
        public string ActivityTitle { get; set; } = string.Empty;
        public string ActivitySubtitle { get; set; } = string.Empty;
        public string ActivityImage { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public List<TeamsFact> Facts { get; set; } = new();
        public bool Markdown { get; set; } = true;
    }

    public class TeamsFact
    {
        public string Name { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }

    public class TeamsPotentialAction
    {
        public string Type { get; set; } = "OpenUri";
        public string Name { get; set; } = string.Empty;
        public List<TeamsTarget> Targets { get; set; } = new();
    }

    public class TeamsTarget
    {
        public string Os { get; set; } = string.Empty;
        public string Uri { get; set; } = string.Empty;
    }

}
