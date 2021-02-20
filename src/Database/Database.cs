using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace Tomoe.Db
{
	public class Database : DbContext
	{
		public DbSet<Guild> Guilds { get; set; }
		public DbSet<GuildUser> Users { get; set; }
		public DbSet<Strike> Strikes { get; set; }
		public DbSet<Assignment> Assignments { get; set; }

		protected override void OnConfiguring(DbContextOptionsBuilder options) => options.UseSqlite("Data Source=res/Tomoe.db");
	}
}
