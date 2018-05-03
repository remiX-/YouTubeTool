﻿using CliWrap;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using Tyrrrz.Extensions;
using YoutubeExplode;
using YoutubeExplode.Models;
using YoutubeExplode.Models.MediaStreams;
using YouTubeTool.Core;
using YouTubeTool.Dialogs;
using YouTubeTool.Services;
using YouTubeTool.Utils.Messages;

namespace YouTubeTool.ViewModels
{
	internal class MainViewModel : ViewModelBase, IMainViewModel
	{
		#region Variables
		private readonly YoutubeClient _client;

		private readonly ISettingsService _settingsService;
		private readonly IUpdateService _updateService;

		private readonly Cli FfmpegCli = new Cli("ffmpeg.exe");

		public HamburgerMenuItem[] AppMenu { get; }

		private string TempDirectoryPath => Path.Combine(_settingsService.OutputFolder.NullIfBlank() ?? Directory.GetCurrentDirectory(), "temp");
		private string OutputDirectoryPath => Path.Combine(_settingsService.OutputFolder.NullIfBlank() ?? Directory.GetCurrentDirectory(), "output");

		#region Fields
		//private bool isResizing;

		private double x;
		private double y;
		private double width;
		private double height;
		private WindowState windowState;

		private string myTitle;
		private string status;
		private string product;

		private List<Video> searchList;

		private bool _isBusy;
		private string _query;
		private Playlist _playlist;
		private Video _video;
		private Channel _channel;
		private double _progress;
		private bool _isProgressIndeterminate;
		#endregion

		#region Properties
		//public bool IsResizing
		//{
		//	get => isResizing;
		//	set
		//	{
		//		Set(ref isResizing, value);
		//		RaisePropertyChanged(() => IsPlaylistDataAvailable);
		//	}
		//}

		public double X
		{
			get => x;
			set => Set(ref x, value);
		}
		public double Y
		{
			get => y;
			set => Set(ref y, value);
		}
		public double Width
		{
			get => width;
			set => Set(ref width, value);
		}
		public double Height
		{
			get => height;
			set => Set(ref height, value);
		}
		public WindowState WindowState
		{
			get => windowState;
			set => Set(ref windowState, value);
		}

		public string MyTitle
		{
			get => myTitle;
			set => Set(ref myTitle, value);
		}

		public string Status
		{
			get => status;
			set
			{
				Set(ref status, value);
				Console.WriteLine(status);
			}
		}

		public string Product
		{
			get => product;
			set => Set(ref product, value);
		}

		public List<Video> SearchList
		{
			get => searchList;
			set
			{
				Set(ref searchList, value);
				RaisePropertyChanged(() => IsDataAvailable);
			}
		}

		public bool IsBusy
		{
			get => _isBusy;
			private set
			{
				Set(ref _isBusy, value);
				GetDataCommand.RaiseCanExecuteChanged();
			}
		}

		public string Query
		{
			get => _query;
			set
			{
				Set(ref _query, value);
				GetDataCommand.RaiseCanExecuteChanged();
			}
		}

		public Playlist Playlist
		{
			get => _playlist;
			private set
			{
				Set(ref _playlist, value);

				if (_playlist == null) return;

				SearchList = _playlist.Videos.ToList();
				RaisePropertyChanged(() => IsDataAvailable);
			}
		}

		public Video Video
		{
			get => _video;
			private set
			{
				Set(ref _video, value);

				if (_video == null) return;

				SearchList = new List<Video> { _video };
				RaisePropertyChanged(() => IsDataAvailable);
			}
		}

		public Channel Channel
		{
			get => _channel;
			private set
			{
				Set(ref _channel, value);
				//RaisePropertyChanged(() => IsChannelDataAvailable);
			}
		}

		public bool IsDataAvailable => SearchList != null && SearchList.Count > 0;

		public double Progress
		{
			get => _progress;
			private set => Set(ref _progress, value);
		}

		public bool IsProgressIndeterminate
		{
			get => _isProgressIndeterminate;
			private set => Set(ref _isProgressIndeterminate, value);
		}
		#endregion

		#region Commands
		public RelayCommand GetDataCommand { get; }
		public RelayCommand<Video> DownloadSongCommand { get; }
		public RelayCommand<Video> DownloadVideoCommand { get; }

		public RelayCommand DownloadAllCommand { get; }

		public RelayCommand ShowSettingsCommand { get; }
		public RelayCommand ShowAboutCommand { get; }

		public RelayCommand ViewLoadedCommand { get; }
		public RelayCommand ViewClosedCommand { get; }
		public RelayCommand ViewSizeChangedCommand { get; }
		#endregion
		#endregion

		#region Window Events
		public MainViewModel(ISettingsService settingsService, IUpdateService updateService)
		{
			// Services
			_settingsService = settingsService;
			_updateService = updateService;

			// Default vars
			MyTitle = "YouTube";
			Status = "Ready";
			Product = $"Made by {AppGlobal.AssemblyCompany} v{AppGlobal.AssemblyVersion.SubstringUntilLast(".")}";
			WindowState = WindowState.Minimized;

			AppMenu = new[]
			{
				new HamburgerMenuItem("AddSeries", "Add Series", PackIconKind.Account),
				new HamburgerMenuItem("Separator", PackIconKind.ServerPlus),
				new HamburgerMenuItem("Settings", PackIconKind.Settings),
				new HamburgerMenuItem("Exit", PackIconKind.ExitToApp)
			};

			// YouTubeExplode init
			_client = new YoutubeClient();

			// Commands
			GetDataCommand = new RelayCommand(GetData, () => !IsBusy && Query.IsNotBlank());
			DownloadSongCommand = new RelayCommand<Video>(o => DownloadSong(o), _ => !IsBusy);
			DownloadVideoCommand = new RelayCommand<Video>(o => DownloadVideo(o), _ => !IsBusy);

			DownloadAllCommand = new RelayCommand(DownloadAll, () => !IsBusy);

			ShowSettingsCommand = new RelayCommand(ShowSettings);
			ShowAboutCommand = new RelayCommand(ShowAbout);

			ViewLoadedCommand = new RelayCommand(ViewLoaded);
			ViewClosedCommand = new RelayCommand(ViewClosed);
			ViewSizeChangedCommand = new RelayCommand(ViewSizeChanged);
		}

		private async void ViewLoaded()
		{
			// Load settings
			_settingsService.Load();

			// Vars
			//Query = "Sa0c1VGoiyc";
			//Query = "https://www.youtube.com/playlist?list=PLyiJecar_vAhAQNqZtbSfCLH-LpUeBnxh";
			X = _settingsService.WindowSettings.X;
			Y = _settingsService.WindowSettings.Y;
			Width = _settingsService.WindowSettings.Width;
			Height = _settingsService.WindowSettings.Height;

			if (X == 0 && Y == 0)
			{
				X = (SystemParameters.PrimaryScreenWidth - Width) / 2;
				Y = (SystemParameters.PrimaryScreenHeight - Height) / 2;
			}

			// Check and prepare update
			try
			{
				var updateVersion = await _updateService.CheckForUpdateAsync();
				if (updateVersion != null)
				{
					MessengerInstance.Send(new ShowNotificationMessage($"v{updateVersion} is available", "GET",
						async () =>
						{
							await _updateService.PrepareUpdateAsync();
							_updateService.NeedRestart = true;

							Application.Current.Shutdown();
						}));
				}
			}
			catch
			{
				MessengerInstance.Send(new ShowNotificationMessage("Failed to perform application auto-update"));
			}
		}

		private void ViewClosed()
		{
			// Save settings
			_settingsService.WindowSettings.X = X;
			_settingsService.WindowSettings.Y = Y;
			_settingsService.WindowSettings.Width = Width;
			_settingsService.WindowSettings.Height = Height;
			_settingsService.WindowSettings.Maximized = WindowState == WindowState.Maximized;
			_settingsService.Save();

			// Finalize updates if available
			_updateService.FinalizeUpdate();
		}

		private void ViewSizeChanged()
		{
			//IsResizing = true;
		}

		public void UpdateWindowState()
		{
			WindowState = _settingsService.WindowSettings.Maximized ? WindowState.Maximized : WindowState.Normal;
		}
		#endregion

		#region Core
		private async void GetData()
		{
			IsBusy = true;
			IsProgressIndeterminate = true;

			// Reset data
			SearchList = null;

			Playlist = null;
			Video = null;
			Channel = null;
			//MediaStreamInfos = null;
			//ClosedCaptionTrackInfos = null;

			var id = Query;
			var tryId = Query;

			// Parse URL if necessary
			try
			{
				if (YoutubeClient.ValidatePlaylistId(Query) || YoutubeClient.TryParsePlaylistId(Query, out tryId))
				{
					Status = $"Working on playlist [{tryId ?? id}]...";
					Playlist = await _client.GetPlaylistAsync(tryId ?? id);
				}
				else if (YoutubeClient.ValidateVideoId(Query) || YoutubeClient.TryParseVideoId(Query, out tryId))
				{
					Status = $"Working on video [{tryId ?? id}]...";
					Video = await _client.GetVideoAsync(tryId ?? id);
				}
				else if (YoutubeClient.ValidateChannelId(Query) || YoutubeClient.TryParseChannelId(Query, out tryId))
				{
					Status = $"Working on channel [{tryId ?? id}]...";
					Channel = await _client.GetChannelAsync(tryId ?? id);
				}

				Status = "Ready";
			}
			catch (Exception ex)
			{
				Status = $"Error: {ex.Message}";
			}

			// Get data
			//MediaStreamInfos = await _client.GetVideoMediaStreamInfosAsync(videoId);
			//ClosedCaptionTrackInfos = await _client.GetVideoClosedCaptionTrackInfosAsync(videoId);\

			IsBusy = false;
			IsProgressIndeterminate = false;
		}

		private async void DownloadAll()
		{
			if (SearchList == null || SearchList.Count == 0) return;

			IsBusy = true;
			IsProgressIndeterminate = true;

			Status = $"Working on {SearchList.Count} videos";

			// Work on the videos
			Console.WriteLine();
			foreach (var video in SearchList)
			{
				await DownloadSongAsync(video);
				Console.WriteLine();
			}

			IsBusy = false;
			IsProgressIndeterminate = false;
		}
		#endregion

		#region YouTube Song DL
		//private async Task DownloadSongPlaylistAsync(string id)
		//{
		//	// Get playlist info
		//	var playlist = await _client.GetPlaylistAsync(id);
		//	Status = $"{playlist.Title} ({playlist.Videos.Count} videos)";

		//	// Work on the videos
		//	Console.WriteLine();
		//	foreach (var video in playlist.Videos)
		//	{
		//		await DownloadSongAsync(video);
		//		Console.WriteLine();
		//	}
		//}

		//private async Task DownloadSongAsync(string id)
		//{
		//	Status = $"Working on video [{id}]...";

		//	// Get video info
		//	var video = await _client.GetVideoAsync(id);

		//	await DownloadSongAsync(video);
		//}

		private async Task DownloadSongAsync(Video video)
		{
			try
			{
				Status = $"Working on video [{video.Title}]...";

				var set = await _client.GetVideoMediaStreamInfosAsync(video.Id);
				var cleanTitle = video.Title.Replace(Path.GetInvalidFileNameChars(), '_');

				// Get highest bitrate audio-only or highest quality mixed stream
				var streamInfo = GetBestAudioStreamInfo(set);

				// Download to temp file
				Status = $"Downloading [{video.Title}]...";

				IsProgressIndeterminate = false;
				var progressHandler = new Progress<double>(p => Progress = p);

				Directory.CreateDirectory(TempDirectoryPath);
				var streamFileExt = streamInfo.Container.GetFileExtension();
				var streamFilePath = Path.Combine(TempDirectoryPath, $"{Guid.NewGuid()}.{streamFileExt}");
				await _client.DownloadMediaStreamAsync(streamInfo, streamFilePath, progressHandler);

				IsProgressIndeterminate = true;

				// Convert to mp3
				Status = $"Converting [{video.Title}]...";
				Directory.CreateDirectory(OutputDirectoryPath);
				var outputFilePath = Path.Combine(OutputDirectoryPath, $"{cleanTitle}.mp3");
				await FfmpegCli.ExecuteAsync($"-i \"{streamFilePath}\" -q:a 0 -map a \"{outputFilePath}\" -y");

				// Delete temp file
				Status = "Deleting temp file...";
				File.Delete(streamFilePath);

				// Edit mp3 metadata
				Status = "Writing metadata...";
				var idMatch = Regex.Match(video.Title, @"^(?<artist>.*?)-(?<title>.*?)$");
				var artist = idMatch.Groups["artist"].Value.Trim();
				var title = idMatch.Groups["title"].Value.Trim();
				using (var meta = TagLib.File.Create(outputFilePath))
				{
					meta.Tag.Performers = new[] { artist };
					meta.Tag.Title = title;
					meta.Save();
				}

				Status = $"Downloaded and converted video [{video.Id}] to [{outputFilePath}]";
			}
			catch (Exception ex)
			{
				Status = $"Error: {ex.Message}";
			}
		}

		private static MediaStreamInfo GetBestAudioStreamInfo(MediaStreamInfoSet set)
		{
			if (set.Audio.Any())
				return set.Audio.WithHighestBitrate();
			if (set.Muxed.Any())
				return set.Muxed.WithHighestVideoQuality();
			throw new Exception("No applicable media streams found for this video");
		}
		#endregion

		#region YouTube Video DL
		//private async Task DownloadVideoAsync(string id)
		//{
		//	Status = $"Working on video [{id}]...";

		//	// Get video info
		//	var video = await _client.GetVideoAsync(id);

		//	await DownloadVideoAsync(video);
		//}

		private async Task DownloadVideoAsync(Video video)
		{
			try
			{
				Status = $"Working on video [{video.Title}]...";

				var cleanTitle = video.Title.Replace(Path.GetInvalidFileNameChars(), '_');

				// Get best streams
				var streamInfoSet = await _client.GetVideoMediaStreamInfosAsync(video.Id);
				var videoStreamInfo = streamInfoSet.Video.WithHighestVideoQuality();
				var audioStreamInfo = streamInfoSet.Audio.WithHighestBitrate();

				// Download streams
				IsProgressIndeterminate = false;
				var progressHandler = new Progress<double>(p => Progress = p);

				Status = $"Downloading [{video.Title}]...";
				Directory.CreateDirectory(TempDirectoryPath);
				var videoStreamFileExt = videoStreamInfo.Container.GetFileExtension();
				var videoStreamFilePath = Path.Combine(TempDirectoryPath, $"VID-{Guid.NewGuid()}.{videoStreamFileExt}");
				await _client.DownloadMediaStreamAsync(videoStreamInfo, videoStreamFilePath, progressHandler);

				Progress = 0;

				var audioStreamFileExt = audioStreamInfo.Container.GetFileExtension();
				var audioStreamFilePath = Path.Combine(TempDirectoryPath, $"AUD-{Guid.NewGuid()}.{audioStreamFileExt}");
				await _client.DownloadMediaStreamAsync(audioStreamInfo, audioStreamFilePath, progressHandler);

				IsProgressIndeterminate = true;

				// Mux streams
				Status = "Combining...";
				Directory.CreateDirectory(OutputDirectoryPath);
				var outputFilePath = Path.Combine(OutputDirectoryPath, $"{cleanTitle}.mp4");
				await FfmpegCli.ExecuteAsync($"-i \"{videoStreamFilePath}\" -i \"{audioStreamFilePath}\" -shortest \"{outputFilePath}\" -y");

				// Delete temp files
				Status = "Deleting temp files...";
				File.Delete(videoStreamFilePath);
				File.Delete(audioStreamFilePath);

				Status = $"Downloaded video [{video.Id}] to [{outputFilePath}]";
			}
			catch (Exception ex)
			{
				Status = $"Error: {ex.Message}";
			}
		}
		#endregion

		#region Commands
		private async void DownloadSong(Video o)
		{
			IsBusy = true;
			IsProgressIndeterminate = true;

			await DownloadSongAsync(o);

			Status = "Ready";

			IsBusy = false;
			IsProgressIndeterminate = false;
			Progress = 0;
		}

		private async void DownloadVideo(Video o)
		{
			IsBusy = true;
			IsProgressIndeterminate = true;

			await DownloadVideoAsync(o);

			Status = "Ready";

			IsBusy = false;
			IsProgressIndeterminate = false;
			Progress = 0;
		}

		private async void ShowSettings() => await DialogHost.Show(new SettingsDialog(), "RootDialog");

		private async void ShowAbout() => await DialogHost.Show(new AboutDialog(), "RootDialog");
		#endregion
	}
}