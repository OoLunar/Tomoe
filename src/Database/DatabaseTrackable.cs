using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OoLunar.Tomoe.Database
{
    /// <summary>
    /// Intended to be used as a base class for database entities. Allows for easy tracking by the <see cref="DatabaseTracker"/>.
    /// </summary>
    public abstract class DatabaseTrackable : IDisposable
    {
        /// <summary>
        /// Whether the object is disposed or not.
        /// </summary>
        public bool IsDisposed { get; internal set; }

        /// <summary>
        /// The hashes of the properties on the object.
        /// </summary>
        private Dictionary<PropertyInfo, int?> PropertyHashes { get; set; } = new();

        /// <summary>
        /// The database assigned id. <see cref="Guid.Empty"/> if not assigned.
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// Starts tracking the object by storing the current values of the properties as hashcodes in <see cref="PropertyHashes"/>.
        /// </summary>
        /// <param name="objProperties">The properties that can be found on the object. Ideally these should be cached.</param>
        public void StartTracking(IEnumerable<PropertyInfo> objProperties)
        {
            // Check if the object is disposed.
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(DatabaseTrackable));
            }

            // Get the hashes of the properties on the object and store them as hashcodes in PropertyHashes.
            Dictionary<PropertyInfo, int?> propertyHashes = new();
            // Lock the current object to prevent any value changes in the properties.
            lock (this)
            {
                foreach (PropertyInfo propertyInfo in objProperties)
                {
                    propertyHashes.Add(propertyInfo, propertyInfo.GetValue(this)?.GetHashCode());
                }
            }

            PropertyHashes = propertyHashes;
        }

        /// <summary>
        /// Compares the hashcodes from the current values of the properties to the hashcodes stored in <see cref="PropertyHashes"/>.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{T}"/> of which properties have changed. Empty if the object hasn't changed.</returns>
        public IEnumerable<PropertyInfo> GetUpdates()
        {
            // Check if the object is disposed.
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(DatabaseTrackable));
            }

            List<PropertyInfo> changedProperties = new();
            // Lock the current object to prevent any value changes in the properties.
            lock (this)
            {
                foreach ((PropertyInfo propertyInfo, int? hashCode) in PropertyHashes)
                {
                    int? newHashCode = propertyInfo.GetValue(this)?.GetHashCode();
                    if (newHashCode != hashCode)
                    {
                        PropertyHashes[propertyInfo] = newHashCode;
                        changedProperties.Add(propertyInfo);
                    }
                }
            }
            return changedProperties.AsEnumerable();
        }

        protected virtual void Dispose(bool disposing)
        {
            // Specifically set IsDisposed so the DatabaseTracker can remove the object from its list.
            if (!IsDisposed)
            {
                PropertyHashes.Clear();
                IsDisposed = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    /// <summary>
    /// Exists for the sole purpose of being able to use <see cref="DatabaseTrackable"/> as a generic type. Should be preferred over <see cref="DatabaseTrackable"/> when possible. Requires an empty constructor.
    /// </summary>
    /// <typeparam name="T">The type to be tracked by <see cref="DatabaseTracker"/>.</typeparam>
    public abstract class DatabaseTrackable<T> : DatabaseTrackable where T : new() { }
}
