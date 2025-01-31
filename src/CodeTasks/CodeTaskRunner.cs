using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using OoLunar.Tomoe.Commands.Owner;
using OoLunar.Tomoe.Database.Models;

namespace OoLunar.Tomoe.CodeTasks
{
    public static class CodeTaskRunner
    {
        private static readonly string _codeTemplate = CompilerUtilities.CopyProjectCsproj();
        private static readonly Dictionary<Ulid, CodeTask> _tasks = [];

        public static async ValueTask<CodeTask> CreateAsync(CodeTaskModel model)
        {
            // Check to see if the build is cached
            string basePath = Path.Combine(Environment.CurrentDirectory, "CodeTasks", model.Id.ToString(), "bin", "Release", ThisAssembly.Project.TargetFramework);
            if (!Directory.Exists(basePath) && await CodeTaskCommand.CompileCodeAsync(model.Id, model.Name, model.Code) is not null)
            {
                throw new InvalidOperationException($"Code task {model.Name} ({model.Id}) failed to compile.");
            }

            // Load the assembly
            Assembly assembly = Assembly.LoadFile($"{basePath}/{model.Id}.dll");

            // Get the type
            Type type = assembly.DefinedTypes.First(x => x.IsSubclassOf(typeof(TaskRunner)));

            // Create the instance
            if (Activator.CreateInstance(type) is not TaskRunner taskRunner)
            {
                throw new InvalidOperationException($"Code task {model.Name} ({model.Id}) failed to create an instance of the task runner.");
            }

            CancellationTokenSource cancellationTokenSource = new();
            CodeTask codeTask = new()
            {
                Model = model,
                TaskRunner = taskRunner,
                CancellationTokenSource = cancellationTokenSource
            };

            _tasks[model.Id] = codeTask;
            return codeTask;
        }

        public static void Run(IServiceProvider serviceProvider, Ulid id)
        {
            if (!_tasks.TryGetValue(id, out CodeTask? task))
            {
                return;
            }

            task.Task = task.TaskRunner.RunAsync(serviceProvider, task.CancellationTokenSource.Token);
        }

        public static IEnumerable<CodeTask> GetTasks() => _tasks.Values;

        public static TaskStatus? GetStatus(Ulid id) => !_tasks.TryGetValue(id, out CodeTask? task) ? null : task.Task?.Status;

        public static async ValueTask StopAsync(Ulid id)
        {
            if (!_tasks.TryGetValue(id, out CodeTask? task))
            {
                return;
            }

            await task.CancellationTokenSource.CancelAsync();
            await Task.Delay(TimeSpan.FromSeconds(1));
            if (task.Task?.Status is not TaskStatus.RanToCompletion and not TaskStatus.Faulted and not TaskStatus.Canceled)
            {
                CancellationTokenSource cancellationTokenSource = new();
                cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(30));
                await Task.Run(() => task.Task, cancellationTokenSource.Token);
            }

            _tasks.Remove(id);
        }
    }
}
