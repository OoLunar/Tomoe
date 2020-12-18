using System;
using System.IO;

namespace Tomoe.Utils {
    public class FileSystem {
        private static Logger Logger = new Logger("Filesystem");
        public static string ProjectRoot = Path.GetFullPath("../../../../", System.AppDomain.CurrentDomain.BaseDirectory).Replace('\\', '/');

        public static bool CreateFile(string file, bool retry = false) {
            if (File.Exists(file)) return true;
            try {
                if (CreateDirectory(Directory.GetParent(file).FullName) == false) {
                    Logger.Warn($"Can't create '{file}' due to failure of parent directory creation.");
                    return false;
                }
                File.Create(file).Close();
                if (retry == true) Logger.Info($"Was able to successfully recover from IO Interruption. '{file}' was created.");
                return true;
            } catch (UnauthorizedAccessException) {
                Logger.Warn($"Can't create '{file}' due to lack of permissions.");
                return false;
            } catch (PathTooLongException) {
                Logger.Warn($"Can't create '{file}' due to the filepath exceeding the system-defined maximum filepath length.");
                return false;
            } catch (IOException) {
                if (retry == false) {
                    Logger.Warn($"Can't create '{file}' due to an IO Interruption. Retrying one more time...");
                    return CreateFile(file, true);
                } else {
                    Logger.Warn($"Failed to create '{file}' twice due to another IO Interruption.");
                    return false;
                }
            }
        }

        public static bool CreateDirectory(string directory) {
            if (Directory.Exists(directory)) return true;
            try {
                Directory.CreateDirectory(directory);
                return true;
            } catch (UnauthorizedAccessException) {
                Logger.Warn($"Can't create '{directory}' due to lack of permissions.");
                return false;
            } catch (PathTooLongException) {
                Logger.Warn($"Can't create '{directory}' due to the filepath exceeding the system-defined maximum filepath length.");
                return false;
            }
        }
    }
}