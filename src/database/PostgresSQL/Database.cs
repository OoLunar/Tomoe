using Tomoe.Database.Interfaces;

namespace Tomoe.Database.Drivers.PostgresSQL {
    public class PostgresSQL : IDatabase {
        private IUser _postgresUser = new PostgresUser();
        private IGuild _postgresGuild = new PostgresGuild();

        public IUser User => _postgresUser;
        public IGuild Guild => _postgresGuild;
    }
}