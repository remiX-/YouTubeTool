using MaterialDesignThemes.Wpf;
using Prism.Mvvm;

namespace YouTubeTool.ViewModels
{
	internal class MainViewModel : BindableBase
	{
		#region Variables
		public HamburgerMenuItem[] AppMenu { get; }

		#region Fields
		private string myTitle;
		#endregion

		#region Properties
		public string MyTitle { get => myTitle; set => SetProperty(ref myTitle, value); }
		#endregion
		#endregion

		public MainViewModel()
		{
			MyTitle = "YouTube Toolerino";

			AppMenu = new[]
			{
				new HamburgerMenuItem("AddSeries", "Add Series", PackIconKind.Account),
				new HamburgerMenuItem("Separator", PackIconKind.ServerPlus),
				new HamburgerMenuItem("Settings", PackIconKind.Settings),
				new HamburgerMenuItem("Exit", PackIconKind.ExitToApp)
			};
		}
	}
}