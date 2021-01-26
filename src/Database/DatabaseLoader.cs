using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;

using Newtonsoft.Json;

using Tomoe.Database.Interfaces;
using Tomoe.Utils;

namespace Tomoe.Database
{
	public class DatabaseLoader
	{
		private static readonly Logger _logger = new("Database.Driver.Setup");

		public IGuild Guild { get; private set; }
		public ITags Tags { get; private set; }
		public IAssignment Assignments { get; private set; }
		public IUser User { get; private set; }
		public IStrikes Strikes { get; private set; }

		[JsonProperty("retry_count")]
		internal static int RetryCount = 5;

		[JsonProperty("driver")]
		private static string selectedDriver;

		[JsonProperty("password")]
		private static string password;

		[JsonProperty("database_name")]
		private static string databaseName;

		[JsonProperty("parameters")]
		private static Dictionary<string, string> parameters;

		[JsonConstructor]
		public DatabaseLoader(string instanceSelectedDriver, string instancePassword, string instanceDatabaseName, Dictionary<string, string> instanceParameters)
		{
			selectedDriver = instanceSelectedDriver;
			password = instancePassword;
			databaseName = instanceDatabaseName;
			parameters = instanceParameters;
		}

		public DatabaseLoader()
		{
			bool foundDriver = false;
			_logger.Debug("Searching classes...");
			Type[] assembly = Assembly.GetEntryAssembly().GetTypes().Where(assembler => assembler.GetInterface(nameof(IDatabase)) != null).ToArray();
			foreach (Type classType in assembly)
			{
				_logger.Debug($"({classType.FullName}) Found class");
				foreach (ConstructorInfo constructor in classType.GetConstructors())
				{
					Type[] classParameters = constructor.GetParameters().Select(parameter => parameter.ParameterType).ToArray();
					if (classParameters.Length != 3) continue;
					if (classType.Name.ToLowerInvariant() == selectedDriver.ToString().ToLowerInvariant())
					{
						_logger.Trace($"({classType.FullName}) Checking if the required properties are set...");
						PropertyInfo guildProperty = classType.GetProperty("Guild");
						PropertyInfo tagProperty = classType.GetProperty("Tags");
						PropertyInfo tasksProperty = classType.GetProperty("Assignments");
						PropertyInfo userProperty = classType.GetProperty("User");
						PropertyInfo strikesProperty = classType.GetProperty("Strikes");
						if (guildProperty != null && tagProperty != null && tasksProperty != null && userProperty != null && strikesProperty != null)
						{
							_logger.Debug($"({classType.FullName}) Successfully maps to the \"{selectedDriver}\" driver.");
							IDatabase database = constructor.Invoke(new object[] { password, databaseName, parameters }) as IDatabase;
							Guild = database.Guild;
							Tags = database.Tags;
							Assignments = database.Assignments;
							User = database.User;
							Strikes = database.Strikes;
							foundDriver = true;
						}
					}
					else _logger.Trace($"({classType.FullName}) Failed to match the requested driver name.");
				}
			}

			if (foundDriver == false) _logger.Critical("No database drivers found. Download some from https://github.com/OoLunar/Tomoe/tree/master/src/database/Drivers.");
		}
	}
}
