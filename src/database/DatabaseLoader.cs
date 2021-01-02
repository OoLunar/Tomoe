using System;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Microsoft.CSharp.RuntimeBinder;
using Newtonsoft.Json;
using Npgsql;
using Tomoe.Utils;

namespace Tomoe.Database
{
	public class DatabaseLoader
	{
		private static readonly Logger Logger = new("Database.Driver.Setup");
		private delegate void DelegateConstructor(string host, int port, string username, string password, string databaseName, SslMode sslMode);

		public Interfaces.IGuild Guild;
		public Interfaces.ITags Tags;
		public Interfaces.IAssignment Assignments;
		public Interfaces.IUser User;
		public Interfaces.IStrikes Strikes;

		[JsonProperty("driver")]
		private static string SelectedDriver { get; set; }

		[JsonProperty("host")]
		private static string Host { get; set; }

		[JsonProperty("port")]
		private static int Port { get; set; }

		[JsonProperty("username")]
		private static string Username { get; set; }

		[JsonProperty("password")]
		private static string Password { get; set; }

		[JsonProperty("database_name")]
		private static string DatabaseName { get; set; }

		[JsonProperty("ssl_mode")]
		private static SslMode SslMode { get; set; }

		[JsonProperty("max_retry_count")]
		internal static int MaxRetryCount { get; set; }

		[JsonConstructor]
		public DatabaseLoader(string selectedDriver, string host, int port, string username, string password, string databaseName, SslMode sslMode)
		{
			SelectedDriver = selectedDriver;
			Host = host;
			Port = port;
			Username = username;
			Password = password;
			DatabaseName = databaseName;
			SslMode = sslMode;
		}

		public DatabaseLoader()
		{
			bool foundDriver = false;
			Logger.Debug("Searching classes...");
			Type[] assembly = Assembly.GetEntryAssembly().GetTypes().Where(asm => asm.GetInterface(typeof(Interfaces.IDatabase).Name) != null).ToArray(); // Single out all classes that inherit Route
			foreach (Type classType in assembly)
			{
				Logger.Debug($"({classType.FullName}) Found class");
				foreach (ConstructorInfo constructor in classType.GetConstructors())
				{
					Type[] parameters = constructor.GetParameters().Select(param => param.ParameterType).ToArray();
					Type[] delegateParameters = typeof(DelegateConstructor).GetMethod("Invoke").GetParameters().Select(param => param.ParameterType).ToArray();
					if (parameters.Length != delegateParameters.Length) continue;
					Logger.Trace($"({classType.FullName}) Found parameters: {string.Join(", ", parameters as object[])}");
					if (parameters.SequenceEqual(delegateParameters))
					{
						Logger.Trace($"({classType.FullName}) Parameters had the correct types and were in the correct order.");
						if (classType.Name.ToLower() == SelectedDriver.ToString().ToLower())
						{
							Logger.Trace($"({classType.FullName}) Checking if the required properties are set...");
							PropertyInfo? guildProperty = classType.GetProperty("Guild");
							PropertyInfo? tagProperty = classType.GetProperty("Tags");
							PropertyInfo? tasksProperty = classType.GetProperty("Assignments");
							PropertyInfo? userProperty = classType.GetProperty("User");
							PropertyInfo? strikesProperty = classType.GetProperty("Strikes");
							if (guildProperty != null && tagProperty != null && tasksProperty != null && userProperty != null && strikesProperty != null)
							{
								Logger.Debug($"({classType.FullName}) Successfully maps to the \"{SelectedDriver}\" driver.");
								Interfaces.IDatabase database = constructor.Invoke(new object[] { Host, Port, Username, Password, DatabaseName, SslMode }) as Interfaces.IDatabase;
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
							Logger.Trace($"({classType.FullName}) Failed to match the requested driver name.");
						}
					}
				}
			}

			if (foundDriver == false)
			{
				Logger.Critical("No database drivers found. Download some from https://github.com/OoLunar/Tomoe/tree/master/src/database/Drivers.");
			}
		}
	}
}
