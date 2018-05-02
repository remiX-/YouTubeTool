using GalaSoft.MvvmLight.CommandWpf;
using System.Windows;
using YoutubeExplode.Models;

namespace YouTubeTool.ViewModels
{
	public interface IMainViewModel
	{
		double X { get; }
		double Y { get; }
		double Width { get; }
		double Height { get; }
		WindowState WindowState { get; }

		string MyTitle { get; }
		string Status { get; }
		string Product { get; }

		bool IsBusy { get; }
		string Query { get; set; }

		Playlist Playlist { get; }
		Video Video { get; }
		Channel Channel { get; }
		bool IsDataAvailable { get; }

		double Progress { get; }
		bool IsProgressIndeterminate { get; }

		RelayCommand GetDataCommand { get; }
		RelayCommand<Video> DownloadSongCommand { get; }
		RelayCommand<Video> DownloadVideoCommand { get; }

		RelayCommand DownloadAllCommand { get; }

		RelayCommand ViewLoadedCommand { get; }
		RelayCommand ViewSizeChangedCommand { get; }
	}
}
