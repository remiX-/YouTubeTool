using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System.IO;
using Tyrrrz.Extensions;
using YouTubeTool.Services;
using WinForms = System.Windows.Forms;

namespace YouTubeTool.ViewModels
{
	internal class SettingsViewModel : ViewModelBase, ISettingsViewModel
	{
		private readonly ISettingsService _settingsService;

		public string DateFormat
		{
			get => _settingsService.DateFormat;
			set => _settingsService.DateFormat = value;
		}

		public string OutputFolder
		{
			get => _settingsService.OutputFolder;
			set => _settingsService.OutputFolder = value;
		}

		public bool IsAutoUpdateEnabled
		{
			get => _settingsService.IsAutoUpdateEnabled;
			set => _settingsService.IsAutoUpdateEnabled = value;
		}

		public RelayCommand BrowseOutputFolderCommand { get; }

		public SettingsViewModel(ISettingsService settingsService)
		{
			_settingsService = settingsService;

			BrowseOutputFolderCommand = new RelayCommand(BrowseOutputFolder);
		}

		private void BrowseOutputFolder()
		{
			WinForms.FolderBrowserDialog fbd = new WinForms.FolderBrowserDialog
			{
				Description = "Select the folder where your series are stored",
				SelectedPath = OutputFolder.NullIfBlank() ?? Directory.GetCurrentDirectory()
			};

			if (fbd.ShowDialog() == WinForms.DialogResult.OK)
			{
				OutputFolder = fbd.SelectedPath;
				RaisePropertyChanged(() => OutputFolder);
			}
		}
	}
}