using CliWrap;
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
using YouTubeTool.Core;
using YouTubeTool.Dialogs;
using YouTubeTool.Enums;
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

		private static readonly string TempDirectoryPath = Path.Combine(Directory.GetCurrentDirectory(), "Temp");
		private static readonly string OutputDirectoryPath = Path.Combine(Directory.GetCurrentDirectory(), "Output");

		#region Fields
		private double x;
		private double y;
		private double width;
		private double height;
		private WindowState windowState;

		private string myTitle;
		private string status;
		private string product;

		private bool _isBusy;
		private string _query;
		private Playlist _playlist;
		private Video _video;
		private Channel _channel;
		private double _progress;
		private bool _isProgressIndeterminate;
		#endregion

		#region Properties
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
				RaisePropertyChanged(() => IsPlaylistDataAvailable);
			}
		}

		public Video Video
		{
			get => _video;
			private set
			{
				Set(ref _video, value);
				RaisePropertyChanged(() => IsVideoDataAvailable);
			}
		}

		public Channel Channel
		{
			get => _channel;
			private set
			{
				Set(ref _channel, value);
				RaisePropertyChanged(() => IsChannelDataAvailable);
			}
		}

		public bool IsPlaylistDataAvailable => Playlist != null;
		public bool IsVideoDataAvailable => Video != null;
		public bool IsChannelDataAvailable => Channel != null;

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

		public RelayCommand ShowSettingsCommand { get; }
		public RelayCommand ShowAboutCommand { get; }

		public RelayCommand ViewLoadedCommand { get; }
		public RelayCommand ViewClosedCommand { get; }
		#endregion
		#endregion

		#region Window Events
		public MainViewModel(ISettingsService settingsService, IUpdateService updateService)
		{
			_settingsService = settingsService;
			_updateService = updateService;

			MyTitle = "YouTube";
			Status = "Ready";
			Product = $"Made by {AppGlobal.AssemblyCompany} v{AppGlobal.AssemblyVersion}";

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

			ShowSettingsCommand = new RelayCommand(ShowSettings);
			ShowAboutCommand = new RelayCommand(ShowAbout);

			ViewLoadedCommand = new RelayCommand(ViewLoaded);
			ViewClosedCommand = new RelayCommand(ViewClosed);
		}

		private async void ViewLoaded()
		{
			// Load settings
			_settingsService.Load();

			// Vars
			Query = "https://www.youtube.com/playlist?list=PLyiJecar_vAhAQNqZtbSfCLH-LpUeBnxh";
			Query = "https://www.youtube.com/watch?v=Sa0c1VGoiyc";
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
			Playlist = null;
			Video = null;
			Channel = null;
			//MediaStreamInfos = null;
			//ClosedCaptionTrackInfos = null;

			var id = Query;

			// Parse URL if necessary
			if (YoutubeClient.ValidatePlaylistId(id) || YoutubeClient.TryParsePlaylistId(Query, out id))
			{
				Status = $"Working on playlist [{id}]...";
				Playlist = await _client.GetPlaylistAsync(id);
			}
			else if (YoutubeClient.ValidateVideoId(id) || YoutubeClient.TryParseVideoId(Query, out id))
			{
				Status = $"Working on video [{id}]...";
				Video = await _client.GetVideoAsync(id);
			}
			else if (YoutubeClient.ValidateChannelId(id) || YoutubeClient.TryParseChannelId(Query, out id))
			{
				Status = $"Working on channel [{id}]...";
				Channel = await _client.GetChannelAsync(id);
			}

			// Get data
			//await DownloadAndConvertPlaylistAsync(playlistId);
			//Video = await _client.GetVideoAsync(videoId);
			//Channel = await _client.GetVideoAuthorChannelAsync(videoId);
			//MediaStreamInfos = await _client.GetVideoMediaStreamInfosAsync(videoId);
			//ClosedCaptionTrackInfos = await _client.GetVideoClosedCaptionTrackInfosAsync(videoId);\

			Status = "Ready";

			IsBusy = false;
			IsProgressIndeterminate = false;
		}
		#endregion

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

		private async void ShowSettings()
		{
			var view = new SettingsDialog();

			//show the dialog
			var result = await DialogHost.Show(view, "RootDialog");
		}

		private async void ShowAbout()
		{
			var view = new AboutDialog();

			//show the dialog
			var result = await DialogHost.Show(view, "RootDialog");
		}
		#endregion

		private bool IsPlaylist(string query)
		{
			return YoutubeClient.ValidatePlaylistId(query) || YoutubeClient.TryParsePlaylistId(Query, out var _);
		}
	}
}