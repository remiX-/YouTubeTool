using GalaSoft.MvvmLight.CommandWpf;
using System.Collections.Generic;
using System.Windows;
using YoutubeExplode.Models;
using YoutubeExplode.Models.MediaStreams;

namespace YouTubeTool.ViewModels
{
	public interface ICutVideoViewModel
	{
		RelayCommand BrowseInputFileCommand { get; }

		RelayCommand GoCommand { get; }
	}
}
