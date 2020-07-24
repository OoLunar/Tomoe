using System;
using System.IO;

namespace Tomoe.Utils {
    public class FileSystem {
        public static bool CreateFile(string file, bool retry = false) {
            if (File.Exists(file)) return true;
            try {
                if (CreateDirectory(Directory.GetParent(file).FullName) == false) {
                    Console.WriteLine($"[Utils.FileSystem.CreateFile] Can't create '{file}' due to failure of parent directory creation.");
                    return false;
                }
                File.Create(file).Close();
                if (retry == true) Console.WriteLine($"[Utils.FileSystem.CreateFile] Was able to successfully recover from IO Interruption. '{file}' was created.");
                return true;
            } catch (UnauthorizedAccessException) {
                Console.WriteLine($"[Utils.FileSystem.CreateFile] Can't create '{file}' due to lack of permissions.");
                return false;
            } catch (PathTooLongException) {
                Console.WriteLine($"[Utils.FileSystem.CreateFile] Can't create '{file}' due to the filepath exceeding the system-defined maximum filepath length.");
                return false;
            } catch (IOException) {
                if (retry == false) {
                    Console.WriteLine($"[Utils.FileSystem.CreateFile] Can't create '{file}' due to an IO Interruption. Retrying one more time...");
                    return CreateFile(file, true);
                } else {
                    Console.WriteLine($"[Utils.FileSystem.CreateFile] Failed to create '{file}' twice due to another IO Interruption.");
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
                Console.WriteLine($"[Utils.FileSystem.CreateDirectory] Can't create '{directory}' due to lack of permissions.");
                return false;
            } catch (PathTooLongException) {
                Console.WriteLine($"[Utils.FileSystem.CreateDirectory] Can't create '{directory}' due to the filepath exceeding the system-defined maximum filepath length.");
                return false;
            }
        }
    }
}