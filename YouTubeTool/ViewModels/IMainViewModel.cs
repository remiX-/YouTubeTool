using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoutubeExplode.Models;
using YoutubeExplode.Models.MediaStreams;

namespace YouTubeTool.ViewModels
{
	internal class IMainViewModel
	{
		bool IsBusy { get; }
		string Query { get; set; }

		Playlist Playlist { get; }
		bool IsDataAvailable { get; }

		double Progress { get; }
		bool IsProgressIndeterminate { get; }

		DelegateCommand GetDataCommand { get; }
		DelegateCommand<string> DownloadSongCommand { get; }
		DelegateCommand<string> DownloadVideoCommand { get; }
	}
}
