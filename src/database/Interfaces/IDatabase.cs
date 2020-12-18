namespace Tomoe.Database.Interfaces {
    public interface IDatabase {
        IUser User { get; }
        IGuild Guild { get; }
        ITags Tags { get; }
        ITasks Tasks { get; }
    }
}