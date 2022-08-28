using System.Threading.Tasks;

namespace OoLunar.Tomoe.Interfaces
{
    public interface IExpirableService<T> where T : IExpireable<T>, new()
    {
        /// <summary>
        /// Asynchronously adds an object to the database. If the object expires soon, the object will be inserted into the database and remain in the memory cache.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="item"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="item"/> has already expired.</exception>
        Task<T> AddAsync(T expirable);

        /// <summary>
        /// Asynchronously removes an object from the database.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="item"/> is <see langword="null"/>.</exception>
        Task<bool> RemoveAsync(T expirable);

        /// <summary>
        /// Checks if any of the items has expired. If so, <see cref="IExpireable{T}.ExpireAsync"/> is called. If <see cref="IExpireable{T}.ExpireAsync"/> returns <see langword="true"/>, the item is removed from the memory cache and the database.
        /// </summary>
        Task CheckExpiredAsync();
    }
}