namespace Tomoe.Database {
    public enum Type {
        Postgres
    }
    public class Database {
        public Tomoe.Database.Interfaces.IDatabase Driver;

        public Database() {
            switch (Program.Config.DatabaseType) {
                case Type.Postgres:
                    Driver = new Tomoe.Database.Drivers.PostgresSQL.PostgresSQL();
                    break;
                default:
                    break;
            }
        }
    }
}