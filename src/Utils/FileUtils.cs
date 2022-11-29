using System;
using System.IO;
using System.Reflection;
using Serilog;

namespace Tomoe.Utils
{
	public static class FileUtils
	{
		public static string GetConfigPath() => Path.Combine(Directory.GetCurrentDirectory(), "res/");

		public static void CreateDefaultConfig()
		{
			string resFolder = GetConfigPath();
			if (!Directory.Exists(resFolder))
			{
				try
				{
					Directory.CreateDirectory(resFolder);
				}
				catch (Exception)
				{
					Log.Warning("Failed to create the resource directory. Unless environment variables or command line arguments are set, default values will be used.");
					return;
				}
			}

			if (!File.Exists(resFolder + "config.json") || File.ReadAllText(resFolder + "config.json").Trim() == string.Empty)
			{
				FileStream configFile;
				try
				{
					configFile = File.Open(resFolder + "config.json", FileMode.Create, FileAccess.Write, FileShare.Read);
				}
				catch (Exception)
				{
					Log.Warning("Failed to read or create the config file. Unless environment variables or command line arguments are set, default values will be used.");
					return;
				}
				StreamReader reader = new(Assembly.GetAssembly(typeof(Program))!.GetManifestResourceStream("config.json") ?? throw new InvalidOperationException("The config file was not embedded into the assembly!"));
				byte[] buffer = new byte[reader.BaseStream.Length];
				reader.BaseStream.Read(buffer, 0, buffer.Length);
				configFile.Write(buffer);
				configFile.Dispose();
			}

			Log.Information("Config file created, please fill it out when you get the chance.");
		}
	}
}
