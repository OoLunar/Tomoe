using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace OoLunar.Tomoe.CodeTasks
{
    public abstract class TaskRunner
    {
        public TaskRunner() { }

        public virtual Task ConfigureAsync(IServiceProvider serviceProvider, ServiceCollection serviceCollection, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public virtual Task RunAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
