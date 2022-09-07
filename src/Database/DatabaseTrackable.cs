using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OoLunar.Tomoe.Database
{
    public abstract class DatabaseTrackable : IDisposable
    {
        public bool IsDisposed { get; internal set; }
        private Dictionary<PropertyInfo, int?> PropertyHashes { get; set; } = new();

        /// <summary>
        /// The database assigned id.
        /// </summary>
        public Guid Id { get; private set; }

        public void StartTracking(IEnumerable<PropertyInfo> objProperties)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(DatabaseTrackable));
            }

            Dictionary<PropertyInfo, int?> propertyHashes = new();
            lock (this)
            {
                foreach (PropertyInfo propertyInfo in objProperties)
                {
                    propertyHashes.Add(propertyInfo, propertyInfo.GetValue(this)?.GetHashCode());
                }
            }

            PropertyHashes = propertyHashes;
        }

        public IEnumerable<PropertyInfo>? GetUpdates()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(DatabaseTrackable));
            }

            List<PropertyInfo> changedProperties = new();
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

    public abstract class DatabaseTrackable<T> : DatabaseTrackable where T : new() { }
}
