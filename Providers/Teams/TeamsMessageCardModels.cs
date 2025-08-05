namespace Easy.Notifications.Providers.Teams
{
    public class TeamsMessageCard
    {
        public string Type { get; set; } = "MessageCard";
        public string Context { get; set; } = "https://schema.org/extensions";
        public string Summary { get; set; }
        public string ThemeColor { get; set; }
        public string Title { get; set; }
        public List<TeamsSection> Sections { get; set; }
        public List<TeamsPotentialAction> PotentialAction { get; set; }
    }

    public class TeamsSection
    {
        public string ActivityTitle { get; set; }
        public string ActivitySubtitle { get; set; }
        public string ActivityImage { get; set; }
        public string Text { get; set; }
        public List<TeamsFact> Facts { get; set; }
        public bool Markdown { get; set; } = true;
    }

    public class TeamsFact
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public class TeamsPotentialAction
    {
        public string Type { get; set; } = "OpenUri";
        public string Name { get; set; }
        public List<TeamsTarget> Targets { get; set; }
    }

    public class TeamsTarget
    {
        public string Os { get; set; }
        public string Uri { get; set; }
    }

}
