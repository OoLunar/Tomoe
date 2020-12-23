using System;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Newtonsoft.Json;
using Npgsql;
using Tomoe.Utils;

namespace Tomoe.Database {
    public class DatabaseLoader {
        private static Logger _logger = new Logger("Database/Driver/Setup");
        private delegate void delegateConstructor(string host, int port, string username, string password, string databaseName, SslMode sslMode);

        public Interfaces.IGuild Guild;
        public Interfaces.ITags Tags;
        public Interfaces.IAssignment Tasks;
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

        [JsonConstructor]
        public DatabaseLoader(string selectedDriver, string host, int port, string username, string password, string databaseName, SslMode sslMode) {
            SelectedDriver = selectedDriver;
            Host = host;
            Port = port;
            Username = username;
            Password = password;
            DatabaseName = databaseName;
            SslMode = sslMode;
        }

        public DatabaseLoader() {
            bool foundDriver = false;
            _logger.Debug("Searching classes...");
            Type[] assembly = Assembly.GetEntryAssembly().GetTypes().Where(asm => asm.GetInterface(typeof(Interfaces.IDatabase).Name) != null).ToArray(); // Single out all classes that inherit Route
            foreach (Type classType in assembly) {
                _logger.Debug($"({classType.FullName}) Found class");
                foreach (ConstructorInfo constructor in classType.GetConstructors()) {
                    Type[] parameters = constructor.GetParameters().Select(param => param.ParameterType).ToArray();
                    Type[] delegateParameters = typeof(delegateConstructor).GetMethod("Invoke").GetParameters().Select(param => param.ParameterType).ToArray();
                    if (parameters.Length != delegateParameters.Length) continue;
                    _logger.Trace($"({classType.FullName}) Found parameters: {string.Join(", ", parameters as object[])}");
                    if (parameters.SequenceEqual(delegateParameters)) {
                        _logger.Trace($"({classType.FullName}) Parameters had the correct types and were in the correct order.");
                        if (classType.Name.ToLower() == SelectedDriver.ToString().ToLower()) {
                            _logger.Trace($"({classType.FullName}) Checking if the required properties are set...");
                            PropertyInfo? guildProperty = classType.GetProperty("Guild");
                            PropertyInfo? tagProperty = classType.GetProperty("Tags");
                            PropertyInfo? tasksProperty = classType.GetProperty("Tasks");
                            PropertyInfo? userProperty = classType.GetProperty("User");
                            PropertyInfo? strikesProperty = classType.GetProperty("Strikes");
                            if (guildProperty != null && tagProperty != null && tasksProperty != null && userProperty != null && strikesProperty != null) {
                                _logger.Debug($"({classType.FullName}) Successfully maps to the \"{SelectedDriver.ToString()}\" driver.");
                                Interfaces.IDatabase database = constructor.Invoke(new object[] { Host, Port, Username, Password, DatabaseName, SslMode }) as Interfaces.IDatabase;
                                Guild = database.Guild;
                                Tags = database.Tags;
                                Tasks = database.Tasks;
                                User = database.User;
                                Strikes = database.Strikes;
                                foundDriver = true;
                            }
                        } else _logger.Trace($"({classType.FullName}) Failed to match the requested driver name.");
                    }
                }
            }
            if (foundDriver == false) _logger.Critical("No database drivers found. Download some from https://github.com/OoLunar/Tomoe/tree/master/src/database/Drivers.");
        }
    }
}