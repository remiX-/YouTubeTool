using System.Windows;
using YouTubeTool.Core;
using YouTubeTool.ViewModels;

namespace YouTubeTool.Windows
{
	public partial class WindowMain : Window
	{
		#region Vars
		MainViewModel MyViewModel;
		#endregion

		public WindowMain()
		{
			InitializeComponent();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			MyViewModel = DataContext as MainViewModel;
			MyViewModel.Query = "https://www.youtube.com/playlist?list=PLyiJecar_vAhAQNqZtbSfCLH-LpUeBnxh";

			Width = AppGlobal.Settings.Windows["Main"].Width;
			Height = AppGlobal.Settings.Windows["Main"].Height;
			Left = (SystemParameters.PrimaryScreenWidth - Width) / 2;
			Top = (SystemParameters.PrimaryScreenHeight - Height) / 2;

			WindowState = AppGlobal.Settings.Windows["Main"].Maximized ? WindowState.Maximized : WindowState.Normal;
		}

		private void Window_Closed(object sender, System.EventArgs e)
		{
			if (WindowState != WindowState.Maximized)
			{
				AppGlobal.Settings.Windows["Main"].Width = Width;
				AppGlobal.Settings.Windows["Main"].Height = Height;
			}

			AppGlobal.Settings.Windows["Main"].Maximized = WindowState == WindowState.Maximized;
			AppGlobal.Settings.Save();
		}
	}
}
