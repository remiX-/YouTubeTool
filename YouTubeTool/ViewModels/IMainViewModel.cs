using GalaSoft.MvvmLight.CommandWpf;
using YoutubeExplode.Models;

namespace YouTubeTool.ViewModels
{
	public interface IMainViewModel
	{
		bool IsBusy { get; }
		string Query { get; set; }

		Playlist Playlist { get; }
		bool IsDataAvailable { get; }

		double Progress { get; }
		bool IsProgressIndeterminate { get; }

		RelayCommand GetDataCommand { get; }
		RelayCommand<string> DownloadSongCommand { get; }
		RelayCommand<string> DownloadVideoCommand { get; }

		RelayCommand ViewLoadedCommand { get; }
	}
}
