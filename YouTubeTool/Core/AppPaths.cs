using System;
using System.IO;

namespace YouTubeTool.Core
{
	public class AppPaths
	{
		// Directories
		public string RootDirectory { get; }

		// Files
		public string SettingsFile { get; }

		public AppPaths()
		{
			RootDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "." + AppGlobal.AssemblyProduct);

			SettingsFile = Path.Combine(RootDirectory, "settings.json");
		}
	}
}
