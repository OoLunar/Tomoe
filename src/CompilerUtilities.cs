using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace OoLunar.Tomoe
{
    public static class CompilerUtilities
    {
        public class PackageReference
        {
            public required string Name { get; init; }
            public string? Version { get; init; }
            public string? Path { get; init; }

            [MemberNotNullWhen(true, nameof(Path))]
            [MemberNotNullWhen(false, nameof(Version))]
            public bool IsProjectReference => Path is not null;
        }

        private static readonly Dictionary<string, string> _embeddedFiles = [];
        private static readonly List<PackageReference> _packageReferences = [];

        static CompilerUtilities()
        {
            Dictionary<string, string> embeddedFiles = [];
            foreach (string resourceFile in typeof(CompilerUtilities).Assembly.GetManifestResourceNames())
            {
                using Stream manifestStream = typeof(CompilerUtilities).Assembly.GetManifestResourceStream(resourceFile) ?? throw new InvalidOperationException($"Failed to get the embedded resource {resourceFile}.");
                using StreamReader streamReader = new(manifestStream);
                embeddedFiles[resourceFile] = streamReader.ReadToEnd();
                if (resourceFile != "Tomoe.Tomoe.csproj")
                {
                    continue;
                }

                List<PackageReference> packageReferences = [];
                using XmlReader xmlReader = XmlReader.Create(new StringReader(embeddedFiles[resourceFile]));
                while (xmlReader.Read())
                {
                    if (xmlReader.NodeType == XmlNodeType.Element && xmlReader.Name == "PackageReference")
                    {
                        string name = xmlReader.GetAttribute("Include")!;
                        string version = xmlReader.GetAttribute("Version")!;
                        packageReferences.Add(new PackageReference
                        {
                            Name = name,
                            Version = version
                        });
                    }
                    else if (xmlReader.NodeType == XmlNodeType.Element && xmlReader.Name == "ProjectReference")
                    {
                        string include = xmlReader.GetAttribute("Include")!.Replace("$(ProjectRoot)", ThisAssembly.Project.ProjectRoot);
                        packageReferences.Add(new PackageReference
                        {
                            Name = Path.GetFileNameWithoutExtension(include),
                            Path = include
                        });
                    }
                }

                packageReferences.Add(new PackageReference
                {
                    Name = "OoLunar.Tomoe",
                    Path = $"{ThisAssembly.Project.ProjectRoot}/src/Tomoe.csproj"
                });
                _packageReferences = packageReferences;
            }

            _embeddedFiles = embeddedFiles;
        }

        public static string GetEmbeddedResource(string resourceName) => _embeddedFiles[resourceName];
        public static IReadOnlyList<PackageReference> GetPackageReferences() => _packageReferences;
        public static string CopyProjectCsproj()
        {
            // Grab the benchmark template from the embedded resource
            StringBuilder projectReferences = new();
            foreach (PackageReference packageReference in GetPackageReferences())
            {
                projectReferences.AppendLine(packageReference.IsProjectReference
                    ? $"        <ProjectReference Include=\"{packageReference.Path}\" />"
                    : $"        <PackageReference Include=\"{packageReference.Name}\" Version=\"{packageReference.Version}\" />");
            }

            return GetEmbeddedResource("Tomoe.Templates.BenchmarkCsproj.template")
                .Replace("{{Configuration}}", ThisAssembly.Project.Configuration)
                .Replace("{{TargetFramework}}", ThisAssembly.Project.TargetFramework)
                .Replace("{{ProjectReferences}}", projectReferences.ToString().Trim());
        }

        public static async ValueTask<(string output, int exitCode)> ExecuteProgramAsync(string command, string args)
        {
            Process process = new()
            {
                StartInfo = new()
                {
                    FileName = command,
                    Arguments = args,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };

            StringBuilder result = new();
            result.AppendLine($"Executing: {command} {args}");
            try
            {
                process.Start();

                CancellationTokenSource source = new(TimeSpan.FromMinutes(1));
                await process.WaitForExitAsync(source.Token);
            }
            catch (Exception error)
            {
                result.AppendLine($"Failed to execute {command} {args}: {error.Message}");
                if (!process.HasExited)
                {
                    process.Kill();
                    result.AppendLine($"Killed {command} {args}");
                }
            }

            if (process.StandardOutput.Peek() > -1)
            {
                result.AppendLine((await process.StandardOutput.ReadToEndAsync()).Trim());
            }

            if (process.StandardError.Peek() > -1)
            {
                result.AppendLine((await process.StandardError.ReadToEndAsync()).Trim());
            }

            return (result.ToString().Trim(), process.ExitCode);
        }
    }
}
