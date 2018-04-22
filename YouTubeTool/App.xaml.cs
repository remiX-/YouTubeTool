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

		private void App_OnStartup(object sender, StartupEventArgs e)
		{
			AppMutex = new Mutex(true, "YouTubeTool", out bool aIsNewInstance);
			if (!aIsNewInstance)
			{
				Current.Shutdown();
			}

			Locator.Init();
		}

		private void App_OnExit(object sender, ExitEventArgs e)
		{
			Locator.Cleanup();
		}
	}
}