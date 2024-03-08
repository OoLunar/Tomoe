namespace OoLunar.Tomoe.Configuration
{
    public sealed record TomoeConfiguration
    {
        public required DiscordConfiguration Discord { get; init; }
        public required DatabaseConfiguration Database { get; init; }
        public LoggerConfiguration Logger { get; init; } = new();
        public string HttpUserAgent { get; init; } = $"OoLunar.Tomoe/{ThisAssembly.Project.Version} ({ThisAssembly.Project.RepositoryUrl})";
    }
}
