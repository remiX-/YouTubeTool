using System.Threading;
using System.Windows;

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
		}
	}
}