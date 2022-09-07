using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using EdgeDB;

namespace OoLunar.Tomoe.Database
{
    public sealed class DatabaseTracker
    {
        public EdgeDBClient EdgeDBClient { get; init; }
        private Dictionary<Type, List<DatabaseTrackable>> TrackedObjects { get; set; } = new();
        private Dictionary<Type, IEnumerable<PropertyInfo>> TypePropertyCache { get; set; } = new();

        public DatabaseTracker(EdgeDBClient edgeDBClient)
            => EdgeDBClient = edgeDBClient ?? throw new ArgumentNullException(nameof(edgeDBClient));

        public void StartTracking<T>(params T[] objects) where T : DatabaseTrackable<T>, new()
        {
            ArgumentNullException.ThrowIfNull(objects);
            if (!TypePropertyCache.TryGetValue(typeof(T), out IEnumerable<PropertyInfo>? typeProperties))
            {
                TypePropertyCache.Add(typeof(T), typeProperties = typeof(T).GetProperties().Where(property => property.SetMethod != null && !property.IsDefined(typeof(EdgeDBIgnoreAttribute))));
            }

            if (!TrackedObjects.TryGetValue(typeof(T), out List<DatabaseTrackable>? trackedObjects))
            {
                TrackedObjects.Add(typeof(T), trackedObjects = new());
            }

            foreach (T obj in objects)
            {
                if (obj.IsDisposed)
                {
                    continue;
                }

                obj.StartTracking(typeProperties);
            }
            trackedObjects.AddRange(objects);
        }

        public IEnumerable<T> StopTracking<T>(params T[] objects) where T : DatabaseTrackable<T>, new()
        {
            if (objects == null)
            {
                throw new ArgumentNullException(nameof(objects));
            }
            else if (objects.Length == 0)
            {
                throw new ArgumentException("No objects were provided.", nameof(objects));
            }

            if (!TrackedObjects.TryGetValue(typeof(T), out List<DatabaseTrackable>? trackedObjects))
            {
                return Enumerable.Empty<T>();
            }

            List<T> removedObjects = new();
            foreach (T obj in objects)
            {
                if (trackedObjects.Remove(obj))
                {
                    removedObjects.Add(obj);
                }
            }

            return removedObjects;
        }

        public async Task UpdateAsync<T>(CancellationToken cancellationToken = default) where T : DatabaseTrackable<T>, new()
        {
            if (!TrackedObjects.TryGetValue(typeof(T), out List<DatabaseTrackable>? trackedObjects))
            {
                return;
            }

            List<T> removedObjects = new();
            foreach (DatabaseTrackable obj in trackedObjects)
            {
                if (obj.IsDisposed)
                {
                    removedObjects.Add((T)obj);
                    continue;
                }

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
                    foreach (PropertyInfo propertyInfo in TypePropertyCache[typeof(T)])
                    {
                        propertyInfo.SetValue(obj, propertyInfo.GetValue(newObj));
                    }
                }
            }

            StopTracking(removedObjects.ToArray());
        }
    }
}
