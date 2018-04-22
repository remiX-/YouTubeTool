﻿using CliWrap;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using MaterialDesignThemes.Wpf;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using Tyrrrz.Extensions;
using YoutubeExplode;
using YoutubeExplode.Models;
using YoutubeExplode.Models.MediaStreams;
using YouTubeTool.Services;
using YouTubeTool.Utils.Messages;

namespace YouTubeTool.ViewModels
{
	internal class MainViewModel : ViewModelBase, IMainViewModel
	{
		#region Variables
		public HamburgerMenuItem[] AppMenu { get; }
		private readonly YoutubeClient _client;

		private readonly ISettingsService _settingsService;
		private readonly IUpdateService _updateService;

		private readonly Cli FfmpegCli = new Cli("ffmpeg.exe");

		private static readonly string TempDirectoryPath = Path.Combine(Directory.GetCurrentDirectory(), "Temp");
		private static readonly string OutputDirectoryPath = Path.Combine(Directory.GetCurrentDirectory(), "Output");

		#region Fields
		private double width;
		private double height;
		private WindowState windowState;

		private string myTitle;
		private string status;

		private bool _isBusy;
		private string _query;
		private Playlist _playlist;
		private Video _video;
		private Channel _channel;
		private double _progress;
		private bool _isProgressIndeterminate;
		#endregion

		#region Properties
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
				RaisePropertyChanged("IsDataAvailable");
			}
		}

		public Video Video
		{
			get => _video;
			private set
			{
				Set(ref _video, value);
				RaisePropertyChanged("IsDataAvailable");
			}
		}

		public Channel Channel
		{
			get => _channel;
			private set
			{
				Set(ref _channel, value);
				RaisePropertyChanged("IsDataAvailable");
			}
		}

		public bool IsDataAvailable => Playlist != null;

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

		public RelayCommand ViewLoadedCommand { get; }
		public RelayCommand ViewClosedCommand { get; }
		#endregion
		#endregion

		public MainViewModel(ISettingsService settingsService, IUpdateService updateService)
		{
			_settingsService = settingsService;
			_updateService = updateService;

			MyTitle = "YouTube";
			Status = "Ready";

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

			ViewLoadedCommand = new RelayCommand(ViewLoaded);
			ViewClosedCommand = new RelayCommand(ViewClosed);
		}

		private async void GetData()
		{
			IsBusy = true;
			IsProgressIndeterminate = true;

			// Reset data
			Playlist = null;
			//Video = null;
			//Channel = null;
			//MediaStreamInfos = null;
			//ClosedCaptionTrackInfos = null;

			// Parse URL if necessary
			if (!YoutubeClient.TryParsePlaylistId(Query, out var playlistId))
				playlistId = Query;

			Status = $"Working on playlist [{playlistId}]...";

			// Get data
			Playlist = await _client.GetPlaylistAsync(playlistId);
			//await DownloadAndConvertPlaylistAsync(playlistId);
			//Video = await _client.GetVideoAsync(videoId);
			//Channel = await _client.GetVideoAuthorChannelAsync(videoId);
			//MediaStreamInfos = await _client.GetVideoMediaStreamInfosAsync(videoId);
			//ClosedCaptionTrackInfos = await _client.GetVideoClosedCaptionTrackInfosAsync(videoId);\

			Status = "Ready";

			IsBusy = false;
			IsProgressIndeterminate = false;
		}

		private async void ViewLoaded()
		{
			// Load settings
			_settingsService.Load();

			// Vars
			Query = "https://www.youtube.com/playlist?list=PLyiJecar_vAhAQNqZtbSfCLH-LpUeBnxh";
			Width = _settingsService.Windows["Main"].Width;
			Height = _settingsService.Windows["Main"].Height;
			WindowState = _settingsService.Windows["Main"].Maximized ? WindowState.Maximized : WindowState.Normal;

			// Check and prepare update
			try
			{
				var updateVersion = await _updateService.CheckPrepareUpdateAsync();
				if (updateVersion != null)
				{
					MessengerInstance.Send(new ShowNotificationMessage(
						$"Update to YouTubeTool v{updateVersion} will be installed when you exit",
						"INSTALL NOW", () =>
						{
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
			_settingsService.Windows["Main"].Width = Width;
			_settingsService.Windows["Main"].Height = Height;
			_settingsService.Windows["Main"].Maximized = WindowState == WindowState.Maximized;
			_settingsService.Save();

			// Finalize updates if available
			_updateService.FinalizeUpdate();
		}

		#region YouTube Song DL
		private async Task DownloadSongPlaylistAsync(string id)
		{
			// Get playlist info
			var playlist = await _client.GetPlaylistAsync(id);
			Status = $"{playlist.Title} ({playlist.Videos.Count} videos)";

			// Work on the videos
			Console.WriteLine();
			foreach (var video in playlist.Videos)
			{
				await DownloadSongAsync(video.Id);
				Console.WriteLine();
			}
		}

		private async Task DownloadSongAsync(string id)
		{
			Status = $"Working on video [{id}]...";

			// Get video info
			var video = await _client.GetVideoAsync(id);

			await DownloadSongAsync(video);
		}

		private async Task DownloadSongAsync(Video video)
		{
			var set = await _client.GetVideoMediaStreamInfosAsync(video.Id);
			var cleanTitle = video.Title.Replace(Path.GetInvalidFileNameChars(), '_');
			Status = $"{video.Title}";

			// Get highest bitrate audio-only or highest quality mixed stream
			var streamInfo = GetBestAudioStreamInfo(set);

			// Download to temp file
			Status = "Downloading...";
			Directory.CreateDirectory(TempDirectoryPath);
			var streamFileExt = streamInfo.Container.GetFileExtension();
			var streamFilePath = Path.Combine(TempDirectoryPath, $"{Guid.NewGuid()}.{streamFileExt}");
			await _client.DownloadMediaStreamAsync(streamInfo, streamFilePath);

			// Convert to mp3
			Status = "Converting...";
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
		private async Task DownloadVideoAsync(string id)
		{
			Status = $"Working on video [{id}]...";

			// Get video info
			var video = await _client.GetVideoAsync(id);

			await DownloadVideoAsync(video);
		}

		private async Task DownloadVideoAsync(Video video)
		{
			var cleanTitle = video.Title.Replace(Path.GetInvalidFileNameChars(), '_');
			Status = $"{video.Title}";

			// Get best streams
			var streamInfoSet = await _client.GetVideoMediaStreamInfosAsync(video.Id);
			var videoStreamInfo = streamInfoSet.Video.WithHighestVideoQuality();
			var audioStreamInfo = streamInfoSet.Audio.WithHighestBitrate();

			// Download streams
			Status = "Downloading...";
			Directory.CreateDirectory(TempDirectoryPath);
			var videoStreamFileExt = videoStreamInfo.Container.GetFileExtension();
			var videoStreamFilePath = Path.Combine(TempDirectoryPath, $"VID-{Guid.NewGuid()}.{videoStreamFileExt}");
			await _client.DownloadMediaStreamAsync(videoStreamInfo, videoStreamFilePath);
			var audioStreamFileExt = audioStreamInfo.Container.GetFileExtension();
			var audioStreamFilePath = Path.Combine(TempDirectoryPath, $"AUD-{Guid.NewGuid()}.{audioStreamFileExt}");
			await _client.DownloadMediaStreamAsync(audioStreamInfo, audioStreamFilePath);

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
		#endregion

		#region Commands
		private async void DownloadSong(Video o)
		{
			await DownloadSongAsync(o.Id);

			Status = "Ready";
		}

		private async void DownloadVideo(Video o)
		{
			await DownloadVideoAsync(o.Id);

			Status = "Ready";
		}
		#endregion
	}
}