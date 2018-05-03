using GalaSoft.MvvmLight.CommandWpf;

namespace YouTubeTool.ViewModels
{
	public interface ISettingsViewModel
	{
		string DateFormat { get; }

		string OutputFolder { get; }

		bool IsAutoUpdateEnabled { get; }

		RelayCommand BrowseOutputFolderCommand { get; }
	}
}