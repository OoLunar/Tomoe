using System;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Newtonsoft.Json;
using Npgsql;
using Tomoe.Utils;

namespace Tomoe.Database {
    public enum DriverType {
        PostgresSQL
    }

    public class Driver {
        private static Logger _logger = new Logger("Database/Driver/Setup");

        public Interfaces.IGuild Guild;
        public Interfaces.ITags Tags;
        public Interfaces.ITasks Tasks;
        public Interfaces.IUser User;

        [JsonProperty("driver")]
        public static DriverType SelectedDriver { get; set; }

        [JsonProperty("host")]
        public static string Host { get; set; }

        [JsonProperty("port")]
        public static int Port { get; set; }

        [JsonProperty("username")]
        public static string Username { get; set; }

        [JsonProperty("password")]
        public static string Password { get; set; }

        [JsonProperty("database_name")]
        public static string DatabaseName { get; set; }

        [JsonProperty("ssl_mode")]
        public static SslMode SslMode { get; set; }

        [JsonConstructor]
        public Driver(DriverType selectedDriver, string host, int port, string username, string password, string databaseName, SslMode sslMode) {
            SelectedDriver = selectedDriver;
            Host = host;
            Port = port;
            Username = username;
            Password = password;
            DatabaseName = databaseName;
            SslMode = sslMode;
        }

        public Driver() {
            _logger.Debug("Searching classes...");
            Type[] assembly = Assembly.GetEntryAssembly().GetTypes().Where(asm => asm.GetInterface(typeof(Interfaces.IDatabase).Name) != null).ToArray(); // Single out all classes that inherit Route
            foreach (Type classType in assembly) {
                _logger.Debug($"Found class: {classType.FullName}");
                foreach (ConstructorInfo constructor in classType.GetConstructors()) {
                    ParameterInfo[] parameters = constructor.GetParameters();
                    _logger.Debug($"Found parameters: {string.Join(", ", parameters as object[])}");
                    if (parameters.Length != 6) continue;
                    parameters = parameters.OrderBy(param => param.Position).ToArray();
                    _logger.Debug("Parameters had the correct types and were in the correct order.");
                    if (classType.Name.ToLower() == SelectedDriver.ToString().ToLower()) {
                        _logger.Debug($"Class {classType.FullName} maps to the {SelectedDriver.ToString()} driver.");
                        PropertyInfo? guildProperty = classType.GetProperty("Guild");
                        PropertyInfo? tagProperty = classType.GetProperty("Tags");
                        PropertyInfo? tasksProperty = classType.GetProperty("Tasks");
                        PropertyInfo? userProperty = classType.GetProperty("User");
                        if (guildProperty != null && tagProperty != null && tasksProperty != null && userProperty != null) {
                            _logger.Debug("Assigning properties now...");
                            Interfaces.IDatabase database = constructor.Invoke(new object[] { Host, Port, Username, Password, DatabaseName, SslMode }) as Interfaces.IDatabase;
                            Guild = database.Guild;
                            Tags = database.Tags;
                            Tasks = database.Tasks;
                            User = database.User;
                        }
                    }
                }
            }
        }
    }
}