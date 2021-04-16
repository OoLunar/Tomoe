using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Tomoe.Db
{
	public class Database : DbContext
	{
		public DbSet<Guild> Guilds { get; set; }

		public Database(DbContextOptions<Database> options) : base(options) { }
	}
}
