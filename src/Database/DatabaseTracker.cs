using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using EdgeDB;

namespace OoLunar.Tomoe.Database
{
    /// <summary>
    /// Tracks the state of the registered objects and updates the database accordingly using generated queries from <see cref="QueryBuilder"/>.
    /// </summary>
    public sealed class DatabaseTracker
    {
        /// <summary>
        /// Used to update or fetch objects from the database.
        /// </summary>
        public EdgeDBClient EdgeDBClient { get; init; }

        /// <summary>
        /// Which objects are currently being tracked.
        /// </summary>
        private Dictionary<Type, List<DatabaseTrackable>> TrackedObjects { get; set; } = new();

        /// <summary>
        /// A cached list of properties for each type.
        /// </summary>
        private Dictionary<Type, IEnumerable<PropertyInfo>> TypePropertyCache { get; set; } = new();

        public DatabaseTracker(EdgeDBClient edgeDBClient)
            => EdgeDBClient = edgeDBClient ?? throw new ArgumentNullException(nameof(edgeDBClient));

        /// <summary>
        /// Starts tracking the object by storing the current values of the properties as hashcodes in <see cref="DatabaseTrackable.PropertyHashes"/>.
        /// </summary>
        /// <param name="objects">The objects to track.</param>
        /// <typeparam name="T">A <see cref="DatabaseTrackable{T}"/> object.</typeparam>
        public void StartTracking<T>(params T[] objects) where T : DatabaseTrackable<T>, new()
        {
            // Basic safety checks.
            if (objects == null)
            {
                throw new ArgumentNullException(nameof(objects));
            }
            else if (objects.Length == 0)
            {
                throw new ArgumentException("No objects to track.", nameof(objects));
            }

            // Get or create the property cache for the type.
            if (!TypePropertyCache.TryGetValue(typeof(T), out IEnumerable<PropertyInfo>? typeProperties))
            {
                TypePropertyCache.Add(typeof(T), typeProperties = typeof(T).GetProperties().Where(property => property.SetMethod != null && !property.IsDefined(typeof(EdgeDBIgnoreAttribute))));
            }

            // Get or create the list of tracked objects for the type.
            if (!TrackedObjects.TryGetValue(typeof(T), out List<DatabaseTrackable>? trackedObjects))
            {
                TrackedObjects.Add(typeof(T), trackedObjects = new());
            }

            // Lock the type list instead of the entire property so other types can still be modified as needed.
            lock (trackedObjects)
            {
                foreach (T obj in objects)
                {
                    // Ignore disposed objects or objects that are already being tracked.
                    if (obj.IsDisposed || trackedObjects.Contains(obj))
                    {
                        continue;
                    }

                    // Store the current state of the object.
                    obj.StartTracking(typeProperties);
                }
            }
            trackedObjects.AddRange(objects);
        }

        /// <summary>
        /// Stops tracking an object and removes it from the tracking list (not the database).
        /// </summary>
        /// <param name="objects">The objects to stop tracking.</param>
        /// <typeparam name="T">A <see cref="DatabaseTrackable{T}"/> object.</typeparam>
        /// <returns>The objects that were successfully removed.</returns>
        public IEnumerable<T> StopTracking<T>(params T[] objects) where T : DatabaseTrackable<T>, new()
        {
            // Basic safety checks.
            if (objects == null)
            {
                throw new ArgumentNullException(nameof(objects));
            }
            else if (objects.Length == 0)
            {
                throw new ArgumentException("No objects were provided.", nameof(objects));
            }

            // Attempt to get the list of tracked objects for the type, return an empty IEnumerable if it doesn't exist.
            if (!TrackedObjects.TryGetValue(typeof(T), out List<DatabaseTrackable>? trackedObjects))
            {
                return Enumerable.Empty<T>();
            }

            List<T> removedObjects = new();
            // Lock the type list instead of the entire property so other types can still be modified as needed.
            lock (trackedObjects)
            {
                foreach (T obj in objects)
                {
                    if (trackedObjects.Remove(obj))
                    {
                        removedObjects.Add(obj);
                    }
                }
            }

            return removedObjects;
        }

        /// <summary>
        /// Updates the database with any changes to the tracked objects.
        /// </summary>
        /// <param name="cancellationToken">A token to cancel the pending network operation.</param>
        /// <typeparam name="T">The <see cref="DatabaseTrackable{T}"/> type to update. Be aware that updating ClassA will not update ClassB</typeparam>
        /// <returns>A task that represents the asynchronous operation. Will contain an <see cref="IEnumerable{T}"/> of the objects that were disposed.</returns>
        public async Task<IEnumerable<T>> UpdateAsync<T>(CancellationToken cancellationToken = default) where T : DatabaseTrackable<T>, new()
        {
            // Attempt to get the list of tracked objects for the type, return if it doesn't exist.
            if (!TrackedObjects.TryGetValue(typeof(T), out List<DatabaseTrackable>? trackedObjects))
            {
                return Enumerable.Empty<T>();
            }

            List<T> removedObjects = new();
            foreach (DatabaseTrackable obj in trackedObjects)
            {
                if (obj.IsDisposed)
                {
                    // Generics allow us to cast the object to the correct type.
                    removedObjects.Add((T)obj);
                    continue;
                }

                // The query to insert or update objects in the database.
                IQueryBuilder? query = null;

                // Object has not yet been inserted into the db.
                if (obj.Id == default) // Ty Syzuna for pointing out that the Guid can be default
                {
                    query = QueryBuilder.Insert((T)obj, true);
                }
                else if (obj.GetUpdates() is IEnumerable<PropertyInfo> updatedProperties)
                {
                    T diffObj = Activator.CreateInstance<T>();
                    foreach (PropertyInfo updatedProperty in updatedProperties)
                    {
                        updatedProperty.SetValue(diffObj, updatedProperty.GetValue(obj));
                    }

                    query = QueryBuilder.Update<T>(oldObj => diffObj, true);
                }

                if (query != null)
                {
                    BuiltQuery builtQuery = query.Build();
                    T newObj = (await EdgeDBClient.QueryAsync<T>(builtQuery.Query, builtQuery.Parameters, Capabilities.Modifications, cancellationToken)).First()!;

                    // Database should return the full object, so we can update the local copy.
                    // Iterate over the object's properties and update the local copy.
                    foreach (PropertyInfo propertyInfo in TypePropertyCache[typeof(T)])
                    {
                        propertyInfo.SetValue(obj, propertyInfo.GetValue(newObj));
                    }
                }
            }

            // Remove the disposed objects from the list.
            return StopTracking(removedObjects.ToArray());
        }
    }
}
