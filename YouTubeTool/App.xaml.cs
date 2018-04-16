using System.IO;
using System.Threading;
using System.Windows;
using YouTubeTool.Core;

namespace YouTubeTool
{
	public partial class App : Application
	{
		private Mutex AppMutex { get; set; }

		public App()
		{

		}

		private void Application_Startup(object sender, StartupEventArgs e)
		{
			AppMutex = new Mutex(true, "YouTubeTool", out bool aIsNewInstance);
			if (!aIsNewInstance)
			{
				Current.Shutdown();
			}

			// Check program directories
			if (!Directory.Exists(AppGlobal.Paths.RootDirectory))
				Directory.CreateDirectory(AppGlobal.Paths.RootDirectory);

			// Load settings file
			if (File.Exists(AppGlobal.Paths.SettingsFile))
				AppGlobal.Settings = AppSettings.Read();
			else
				AppGlobal.Settings = new AppSettings(true);
		}
	}
}