using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading;
using System.Threading.Tasks;

namespace OoLunar.Tomoe.Interfaces
{
    /// <summary>
    /// Disposes of an object after the specified amount of time.
    /// </summary>
    public interface IExpirable<T>
    {
        /// <summary>
        /// The id assigned to the object.
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// When the object is set to expire. This is allowed to change.
        /// </summary>
        DateTime ExpiresAt { get; set; }

        [NotMapped]
        SemaphoreSlim IsExecuting { get; }

        [NotMapped]
        bool HasExecuted { get; set; }

        /// <summary>
        /// Attempts to expire the object.
        /// </summary>
        /// <param name="serviceProvider">The service provider used to grab any services the object may require.</param>
        /// <param name="cancellationToken">The cancellation token used to cancel the method if required.</param>
        /// <returns><see langword="true"/> if the object was executed successfully, <see langword="false"/> if the object should not expire and should be executed again at a later time.</returns>
        Task ExpireAsync(IServiceProvider serviceProvider);
    }
}
