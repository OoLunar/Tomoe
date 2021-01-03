using System;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Newtonsoft.Json;
using Npgsql;
using Tomoe.Utils;
using Tomoe.Database.Interfaces;

namespace Tomoe.Database
{
	public class DatabaseLoader
	{
		private static readonly Logger _logger = new("Database.Driver.Setup");
		private delegate void DelegateConstructor(string host, int port, string username, string password, string databaseName, SslMode sslMode);

		public IGuild Guild { get; private set; }
		public ITags Tags { get; private set; }
		public IAssignment Assignments { get; private set; }
		public IUser User { get; private set; }
		public IStrikes Strikes { get; private set; }

		[JsonProperty("driver")]
		private static string selectedDriver;

		[JsonProperty("host")]
		private static string host;

		[JsonProperty("port")]
		private static int port;

		[JsonProperty("username")]
		private static string username;

		[JsonProperty("password")]
		private static string password;

		[JsonProperty("database_name")]
		private static string databaseName;

		[JsonProperty("ssl_mode")]
		private static SslMode sslMode;

		[JsonProperty("max_retry_count")]
		internal static int MaxRetryCount;

		[JsonConstructor]
		public DatabaseLoader(string instanceSelectedDriver, string instanceHost, int instancePort, string instanceUsername, string instancePassword, string instanceDatabaseName, SslMode instanceSslMode)
		{
			selectedDriver = instanceSelectedDriver;
			host = instanceHost;
			port = instancePort;
			username = instanceUsername;
			password = instancePassword;
			databaseName = instanceDatabaseName;
			sslMode = instanceSslMode;
		}

		public DatabaseLoader()
		{
			bool foundDriver = false;
			_logger.Debug("Searching classes...");
			Type[] assembly = Assembly.GetEntryAssembly().GetTypes().Where(asm => asm.GetInterface(typeof(IDatabase).Name) != null).ToArray(); // Single out all classes that inherit Route
			foreach (Type classType in assembly)
			{
				_logger.Debug($"({classType.FullName}) Found class");
				foreach (ConstructorInfo constructor in classType.GetConstructors())
				{
					Type[] parameters = constructor.GetParameters().Select(param => param.ParameterType).ToArray();
					Type[] delegateParameters = typeof(DelegateConstructor).GetMethod("Invoke").GetParameters().Select(param => param.ParameterType).ToArray();
					if (parameters.Length != delegateParameters.Length) continue;
					_logger.Trace($"({classType.FullName}) Found parameters: {string.Join(", ", parameters as object[])}");
					if (parameters.SequenceEqual(delegateParameters))
					{
						_logger.Trace($"({classType.FullName}) Parameters had the correct types and were in the correct order.");
						if (classType.Name.ToLower() == selectedDriver.ToString().ToLower())
						{
							_logger.Trace($"({classType.FullName}) Checking if the required properties are set...");
							PropertyInfo? guildProperty = classType.GetProperty("Guild");
							PropertyInfo? tagProperty = classType.GetProperty("Tags");
							PropertyInfo? tasksProperty = classType.GetProperty("Assignments");
							PropertyInfo? userProperty = classType.GetProperty("User");
							PropertyInfo? strikesProperty = classType.GetProperty("Strikes");
							if (guildProperty != null && tagProperty != null && tasksProperty != null && userProperty != null && strikesProperty != null)
							{
								_logger.Debug($"({classType.FullName}) Successfully maps to the \"{selectedDriver}\" driver.");
								IDatabase database = constructor.Invoke(new object[] { host, port, username, password, databaseName, sslMode }) as IDatabase;
								Guild = database.Guild;
								Tags = database.Tags;
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
			}

			if (foundDriver == false)
			{
				_logger.Critical("No database drivers found. Download some from https://github.com/OoLunar/Tomoe/tree/master/src/database/Drivers.");
			}
		}
	}
}
