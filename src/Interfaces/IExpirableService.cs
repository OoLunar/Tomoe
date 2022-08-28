using System;
using System.Threading.Tasks;

namespace OoLunar.Tomoe.Interfaces
{
    public interface IExpirableService<T> where T : IExpirable<T>, new()
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
        /// Attempt to get an item from database.
        /// </summary>
        /// <param name="id">The id of the expirable.</param>
        /// <param name="item">The expirable itself.</param>
        /// <returns><see langword="true"/> if the expirable was found, <see langword="false"/> otherwise.</returns>
        Task<T?> TryGetItemAsync(Guid id);

        /// <summary>
        /// Checks if any of the items has expired. If so, <see cref="IExpirable{T}.ExpireAsync"/> is called. If <see cref="IExpirable{T}.ExpireAsync"/> returns <see langword="true"/>, the item is removed from the memory cache and the database.
        /// </summary>
        Task CheckExpiredAsync();
    }
}
