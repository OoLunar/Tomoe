using Microsoft.EntityFrameworkCore;

namespace Tomoe.Models
{
    public class DatabaseContext : DbContext
    {
        public DbSet<PollModel> Polls { get; init; } = null!;

        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }
    }
}