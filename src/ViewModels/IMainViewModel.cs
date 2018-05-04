using GalaSoft.MvvmLight.CommandWpf;
using System.Collections.Generic;
using System.Windows;
using YoutubeExplode.Models;
using YoutubeExplode.Models.MediaStreams;

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
		string Query { get; }

		List<Video> SearchList { get; }

		Playlist Playlist { get; }
		Video Video { get; }
		MediaStreamInfoSet MediaStreamInfos { get; }

		bool IsSearchDataAvailable { get; }
		bool IsMediaStreamDataAvailable { get; }

		double Progress { get; }
		bool IsProgressIndeterminate { get; }

		// Core Commands
		RelayCommand GetDataCommand { get; }

		RelayCommand DownloadAllCommand { get; }

		RelayCommand<Video> DownloadSongCommand { get; }
		RelayCommand<Video> DownloadVideoCommand { get; }

		RelayCommand<MediaStreamInfo> DownloadMediaStreamCommand { get; }

		// Dialog Commands
		RelayCommand ShowSettingsCommand { get; }
		RelayCommand ShowAboutCommand { get; }

		// ListBox Events
		RelayCommand<Video> SelectionChangedCommand { get;}

		// Window Events
		RelayCommand ViewLoadedCommand { get; }
		RelayCommand ViewClosedCommand { get; }
	}
}
