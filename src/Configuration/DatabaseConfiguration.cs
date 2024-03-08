using System.Diagnostics;

namespace OoLunar.Tomoe.Configuration
{
    public sealed record DatabaseConfiguration
    {
        public string Host { get; init; } = "localhost";
        public int Port { get; init; } = 5432;
        public string Username { get; init; } = "postgres";
        public string Password { get; init; } = "postgres";
        public string Database { get; init; } = "tomoe";
        public int CommandTimeout { get; init; } = 5;
        public bool IncludeErrorDetail { get; init; } = Debugger.IsAttached;
        public int ExpireInterval { get; init; } = 30;
    }
}
