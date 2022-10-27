using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;
using Tomoe.Utilities.Configs;

namespace Tomoe.Db
{
    public class DatabaseFactory : IDesignTimeDbContextFactory<Database>
    {
        public Database CreateDbContext(string[] args)
        {
            ServiceCollection services = new();
            Config config = Config.LoadAsync().GetAwaiter().GetResult();
            config.Logger.Load(services);
            config.Database.LoadAsync(services).GetAwaiter().GetResult();
            return services.BuildServiceProvider().GetService<Database>();
        }
    }
}
