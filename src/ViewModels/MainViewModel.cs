using CliWrap;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
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

		private MediaStreamInfoSet _mediaStreamInfos;

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

		public List<Video> SearchList
		{
			get => searchList;
			set
			{
				Set(ref searchList, value);
				RaisePropertyChanged(() => IsSearchDataAvailable);
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
			}
		}

		public Video Video
		{
			get => _video;
			private set
			{
				Set(ref _video, value);
			}
		}

		public MediaStreamInfoSet MediaStreamInfos
		{
			get => _mediaStreamInfos;
			private set
			{
				Set(ref _mediaStreamInfos, value);
				RaisePropertyChanged(() => IsMediaStreamDataAvailable);
			}
		}

		public bool IsSearchDataAvailable => SearchList != null && SearchList.Count > 0;
		
		public bool IsMediaStreamDataAvailable => MediaStreamInfos != null;

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

		public RelayCommand DownloadAllCommand { get; }

		public RelayCommand<Video> DownloadSongCommand { get; }
		public RelayCommand<Video> DownloadVideoCommand { get; }

		public RelayCommand<MediaStreamInfo> DownloadMediaStreamCommand { get; }

		public RelayCommand CancelCurrentTaskCommand { get; }

		public RelayCommand ShowSettingsCommand { get; }
		public RelayCommand ShowAboutCommand { get; }

		public RelayCommand<Video> SelectionChangedCommand { get; }

		public RelayCommand ViewLoadedCommand { get; }
		public RelayCommand ViewClosedCommand { get; }
		#endregion

		private CancellationTokenSource CancelTokenSource;
		private CancellationToken CancelToken;

		private Progress<double> AppProgressHandler;
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

			AppProgressHandler = new Progress<double>(p => Progress = p);

			// Core Commands
			GetDataCommand = new RelayCommand(GetData, () => !IsBusy && Query.IsNotBlank());

			DownloadAllCommand = new RelayCommand(DownloadAll, () => !IsBusy);
			DownloadSongCommand = new RelayCommand<Video>(o => DownloadSong(o), _ => !IsBusy);
			DownloadVideoCommand = new RelayCommand<Video>(o => DownloadVideo(o), _ => !IsBusy);

			DownloadMediaStreamCommand = new RelayCommand<MediaStreamInfo>(DownloadMediaStream, _ => !IsBusy);

			CancelCurrentTaskCommand = new RelayCommand(CancelCurrentTask, () => IsBusy);

			// Dialog Commands
			ShowSettingsCommand = new RelayCommand(ShowSettings);
			ShowAboutCommand = new RelayCommand(ShowAbout);

			// ListBox Events
			SelectionChangedCommand = new RelayCommand<Video>(o => SelectionChanged(o), _ => !IsBusy);

			// Window Events
			ViewLoadedCommand = new RelayCommand(ViewLoaded);
			ViewClosedCommand = new RelayCommand(ViewClosed);
		}

		private async void ViewLoaded()
		{
			// Load settings
			_settingsService.Load();

			// Vars
			//Query = "Sa0c1VGoiyc";
			Query = "https://www.youtube.com/playlist?list=PLyiJecar_vAhAQNqZtbSfCLH-LpUeBnxh";
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
			_settingsService.WindowSettings.Maximized = WindowState == WindowState.Maximized;

			if (WindowState == WindowState.Normal)
			{
				_settingsService.WindowSettings.Width = Width;
				_settingsService.WindowSettings.Height = Height;
			}

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
		private async Task GetVideoData()
		{
			AppBusyState(true);

			// Reset data
			SearchList = null;
			Playlist = null;

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
					var video = await _client.GetVideoAsync(tryId ?? id);

					SearchList = new List<Video> { video };
				}

				Status = "Ready";
			}
			catch (Exception ex)
			{
				Status = $"Error: {ex.Message}";
			}

			AppReadyState();
		}

		private async Task DownloadAllAsMp3()
		{
			AppBusyState(true);

			Status = $"Working on {SearchList.Count} videos";

			// Work on the videos
			Console.WriteLine();
			foreach (var video in SearchList)
			{
				await DownloadSongAsync(video);
				Console.WriteLine();
			}

			AppReadyState();
		}

		private async Task DownloadVideoAsMp3(Video video)
		{
			AppBusyState(true);

			await DownloadSongAsync(video);

			AppReadyState();
		}

		private async Task DownloadVideoAsMp4(Video video)
		{
			AppBusyState();

			await DownloadVideoAsync(video);

			AppReadyState();
		}
		#endregion

		#region Downloading
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

				Directory.CreateDirectory(TempDirectoryPath);
				var streamFileExt = streamInfo.Container.GetFileExtension();
				var streamFilePath = Path.Combine(TempDirectoryPath, $"{Guid.NewGuid()}.{streamFileExt}");
				await _client.DownloadMediaStreamAsync(streamInfo, streamFilePath, AppProgressHandler, CancelToken);

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

				Status = $"Downloading [{video.Title}]...";
				Directory.CreateDirectory(TempDirectoryPath);
				var videoStreamFileExt = videoStreamInfo.Container.GetFileExtension();
				var videoStreamFilePath = Path.Combine(TempDirectoryPath, $"VID-{Guid.NewGuid()}.{videoStreamFileExt}");
				await _client.DownloadMediaStreamAsync(videoStreamInfo, videoStreamFilePath, AppProgressHandler);

				Progress = 0;

				var audioStreamFileExt = audioStreamInfo.Container.GetFileExtension();
				var audioStreamFilePath = Path.Combine(TempDirectoryPath, $"AUD-{Guid.NewGuid()}.{audioStreamFileExt}");
				await _client.DownloadMediaStreamAsync(audioStreamInfo, audioStreamFilePath, AppProgressHandler);

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

		private async Task DownloadMediaStreamAsync(MediaStreamInfo info)
		{
			AppBusyState();

			Directory.CreateDirectory(OutputDirectoryPath);
			var fileExt = info.Container.GetFileExtension();
			var defaultFileName = $"{Video.Title}.{fileExt}".Replace(Path.GetInvalidFileNameChars(), '_');
			var outputFilePath = Path.Combine(OutputDirectoryPath, defaultFileName);

			// Download to file
			await _client.DownloadMediaStreamAsync(info, outputFilePath, AppProgressHandler);

			AppReadyState();
		}
		#endregion

		#region Commands
		private async void GetData() => await GetVideoData();

		private async void DownloadAll() => await DownloadAllAsMp3();

		private async void DownloadSong(Video video) => await DownloadVideoAsMp3(video);

		private async void DownloadVideo(Video video) => await DownloadVideoAsMp4(video);

		private async void DownloadMediaStream(MediaStreamInfo info) => await DownloadMediaStreamAsync(info);

		private void CancelCurrentTask() => CancelTokenSource.Cancel();

		private async void ShowSettings() => await DialogHost.Show(new SettingsDialog(), "RootDialog");

		private async void ShowAbout() => await DialogHost.Show(new AboutDialog(), "RootDialog");

		private async void SelectionChanged(Video video)
		{
			AppBusyState(true);

			Video = video;
			MediaStreamInfos = null;
			MediaStreamInfos = await _client.GetVideoMediaStreamInfosAsync(video.Id);

			AppReadyState();
		}
		#endregion

		private void AppBusyState(bool progressIndeterminate = false)
		{
			IsBusy = true;
			IsProgressIndeterminate = progressIndeterminate;
			Progress = 0;

			CancelTokenSource = new CancellationTokenSource();
			CancelToken = CancelTokenSource.Token;
		}

		private void AppReadyState()
		{
			CancelTokenSource.Dispose();

			Status = "Ready";

			IsBusy = false;
			IsProgressIndeterminate = false;
			Progress = 0;
		}
	}
}