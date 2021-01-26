using System;
using System.IO;
using System.Reflection;

namespace Tomoe.Utils
{
	internal class FileSystem
	{
#if DEBUG
#pragma warning disable IL3000
		public static readonly string ProjectRoot = Path.GetDirectoryName(Path.GetFullPath(Path.Join(Assembly.GetExecutingAssembly().Location, "../../../../../")));
#else
        // Places the log directory right next to the executable.
        public static readonly string ProjectRoot = Path.GetDirectoryName(Path.GetFullPath(System.AppContext.BaseDirectory));
#endif
		private static readonly Logger _logger = new("Filesystem");

		public static bool CreateFile(string file, bool retry = false)
		{
			if (File.Exists(file)) return true;
			try
			{
				if (CreateDirectory(Directory.GetParent(file).FullName) == false)
				{
					_logger.Warn($"Can't create \"{file}\" due to failure of parent directory creation.");
					return false;
				}
				File.Create(file).Close();
				if (retry) _logger.Info($"Was able to successfully recover from IO Interruption. \"{file}\" was created.");
				return true;
			}
			catch (UnauthorizedAccessException)
			{
				_logger.Warn($"Can't create \"{file}\" due to lack of permissions.");
				return false;
			}
			catch (PathTooLongException)
			{
				_logger.Warn($"Can't create \"{file}\" due to the filepath exceeding the system-defined maximum filepath length.");
				return false;
			}
			catch (IOException)
			{
				if (!retry)
				{
					_logger.Warn($"Can't create \"{file}\" due to an IO Interruption. Retrying one more time...");
					return CreateFile(file, true);
				}
				else
				{
					_logger.Warn($"Failed to create \"{file}\" twice due to another IO Interruption.");
					return false;
				}
			}
		}

		public static bool CreateDirectory(string directory)
		{
			if (Directory.Exists(directory)) return true;
			try
			{
				_ = Directory.CreateDirectory(directory);
				return true;
			}
			catch (UnauthorizedAccessException)
			{
				_logger.Warn($"Can't create \"{directory}\" due to lack of permissions.");
				return false;
			}
			catch (PathTooLongException)
			{
				_logger.Warn($"Can't create \"{directory}\" due to the filepath exceeding the system-defined maximum filepath length.");
				return false;
			}
		}
	}
}
