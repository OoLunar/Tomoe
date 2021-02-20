using System;
using System.Linq;
using System.Reflection;

using Tomoe.Database.Interfaces;
using Tomoe.Utils;

namespace Tomoe.Database
{
	public class DatabaseLoader : IDisposable
	{
		private static readonly Logger _logger = new("Database.Driver.Setup");

		internal IDatabase Database { get; private set; }
		public IGuild Guild { get; private set; }
		public ITags Tags { get; private set; }
		public IAssignment Assignments { get; private set; }
		public IUser User { get; private set; }
		public IStrikes Strikes { get; private set; }

		public void Dispose()
		{
			Database.Dispose();
			GC.SuppressFinalize(this);
		}

		public DatabaseLoader()
		{
			bool foundDriver = false;
			_logger.Debug("Searching classes...");
			Type[] assembly = Assembly.GetEntryAssembly().GetTypes().Where(asm => asm.GetInterface(nameof(IDatabase)) != null).ToArray(); // Single out all classes that inherit Route
			foreach (Type classType in assembly)
			{
				if (foundDriver) break;
				_logger.Debug($"({classType.FullName}) Found class");
				foreach (ConstructorInfo constructor in classType.GetConstructors())
				{
					Type[] classParameters = constructor.GetParameters().Select(param => param.ParameterType).ToArray();
					if (classParameters.Length != 3) continue;
					_logger.Trace($"({classType.FullName}) Found parameters: {string.Join(", ", classParameters as object[])}");
					_logger.Trace($"({classType.FullName}) Parameters had the correct types and were in the correct order.");
					if (classType.Name.ToLower() == Config.Database.Driver.ToString().ToLower())
					{
						_logger.Trace($"({classType.FullName}) Checking if the required properties are set...");
						PropertyInfo guildProperty = classType.GetProperty("Guild");
						PropertyInfo tagProperty = classType.GetProperty("Tags");
						PropertyInfo tasksProperty = classType.GetProperty("Assignments");
						PropertyInfo userProperty = classType.GetProperty("User");
						PropertyInfo strikesProperty = classType.GetProperty("Strikes");
						if (guildProperty != null && tagProperty != null && tasksProperty != null && userProperty != null && strikesProperty != null)
						{
							_logger.Debug($"({classType.FullName}) Successfully maps to the \"{Config.Database.Driver}\" driver.");
							IDatabase database = constructor.Invoke(new object[] { Config.Database.Password, Config.Database.DatabaseName, Config.Database.Parameters }) as IDatabase;
							Database = database;
							Guild = database.Guild;
							//Tags = database.Tags;
							Assignments = database.Assignments;
							User = database.User;
							Strikes = database.Strikes;
							foundDriver = true;
						}
					}
					else
					{
						_logger.Trace($"({classType.FullName}) Failed to match the requested driver name.");
					}
				}
			}

			if (foundDriver == false)
			{
				_logger.Critical("No database drivers found. Download some from https://github.com/OoLunar/Tomoe/tree/master/src/Database/Drivers.");
			}
		}
	}
}
