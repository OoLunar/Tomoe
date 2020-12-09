using Tomoe.Database.Interfaces;

namespace Tomoe.Database.Drivers.PostgresSQL {
    public class PostgresSQL : IDatabase {
        private IUser _postgresUser = new PostgresUser();
        private IGuild _postgresGuild = new PostgresGuild();
        private ITags _postgresTags = new PostgresTags();

        public IUser User => _postgresUser;
        public IGuild Guild => _postgresGuild;
        public ITags Tags => _postgresTags;
    }
}