using System.Windows;
using YouTubeTool.Core;
using YouTubeTool.Services;
using YouTubeTool.ViewModels;

namespace YouTubeTool.Windows
{
	public partial class WindowMain : Window
	{
		#region Vars
		private MainViewModel MyViewModel;

		private readonly IUpdateService _updateService = new UpdateService();
		#endregion

		public WindowMain()
		{
			InitializeComponent();
		}

		private async void Window_Loaded(object sender, RoutedEventArgs e)
		{
			MyViewModel = DataContext as MainViewModel;
			MyViewModel.Query = "https://www.youtube.com/playlist?list=PLyiJecar_vAhAQNqZtbSfCLH-LpUeBnxh";

			Width = AppGlobal.Settings.Windows["Main"].Width;
			Height = AppGlobal.Settings.Windows["Main"].Height;
			Left = (SystemParameters.PrimaryScreenWidth - Width) / 2;
			Top = (SystemParameters.PrimaryScreenHeight - Height) / 2;

			WindowState = AppGlobal.Settings.Windows["Main"].Maximized ? WindowState.Maximized : WindowState.Normal;

			// Check and prepare update
			try
			{
				var updateVersion = await _updateService.CheckPrepareUpdateAsync();
				if (updateVersion != null)
				{
					MainSnackbar.MessageQueue.Enqueue($"Update to DiscordChatExporter v{updateVersion} will be installed when you exit", "INSTALL NOW",
						() =>
						{
							_updateService.NeedRestart = true;
							Application.Current.Shutdown();
						});
				}
			}
			catch
			{
				MainSnackbar.MessageQueue.Enqueue("Failed to perform application auto-update");
			}
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

			// Finalize updates if available
			_updateService.FinalizeUpdate();
		}
	}
}
