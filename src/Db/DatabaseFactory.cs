namespace Tomoe.Db
{
    using Microsoft.EntityFrameworkCore.Design;
    using Microsoft.Extensions.DependencyInjection;
    using Tomoe.Utilities.Configs;

    public class DatabaseFactory : IDesignTimeDbContextFactory<Database>
    {
        public Database CreateDbContext(string[] args)
        {
            ServiceCollection services = new();
            Config config = Config.Load().GetAwaiter().GetResult();
            config.Logger.Load(services);
            config.Database.Load(services).GetAwaiter().GetResult();
            return services.BuildServiceProvider().GetService<Database>();
        }
    }
}