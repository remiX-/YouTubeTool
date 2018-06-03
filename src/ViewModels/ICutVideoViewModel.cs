using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Windows;
using YoutubeExplode.Models;
using YoutubeExplode.Models.MediaStreams;

namespace YouTubeTool.ViewModels
{
	public interface ICutVideoViewModel
	{
		string InputFile { get; }
		string OutputFile { get; }

		bool TimeSlice { get; }
		bool RemoveAudio { get; }

		TimeSpan StartTime { get; }
		TimeSpan EndTime { get; }

		RelayCommand BrowseInputFileCommand { get; }
		RelayCommand BrowseOutputFileCommand { get; }

		RelayCommand GoCommand { get; }
	}
}
