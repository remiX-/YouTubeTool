using CliWrap;
using MaterialDesignThemes.Wpf;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Tyrrrz.Extensions;
using YoutubeExplode;
using YoutubeExplode.Models;
using YoutubeExplode.Models.MediaStreams;

namespace YouTubeTool.ViewModels
{
	internal class MainViewModel : BindableBase
	{
		#region Variables
		public HamburgerMenuItem[] AppMenu { get; }
		private readonly YoutubeClient _client;

		private readonly Cli FfmpegCli = new Cli("ffmpeg.exe");

		#region Fields
		private string myTitle;
		private string status;

		private bool _isBusy;
		private string _query;
		private Playlist _playlist;
		private Video _video;
		private Channel _channel;
		//private MediaStreamInfoSet _mediaStreamInfos;
		//private IReadOnlyList<ClosedCaptionTrackInfo> _closedCaptionTrackInfos;
		private double _progress;
		private bool _isProgressIndeterminate;
		#endregion

		#region Properties
		public string MyTitle
		{
			get => myTitle;
			set => SetProperty(ref myTitle, value);
		}

		public string Status
		{
			get => status;
			set { SetProperty(ref status, value); Console.WriteLine(status); }
		}

		public bool IsBusy
		{
			get => _isBusy;
			private set
			{
				SetProperty(ref _isBusy, value);
				GetDataCommand.RaiseCanExecuteChanged();
			}
		}

		public string Query
		{
			get => _query;
			set
			{
				SetProperty(ref _query, value);
				GetDataCommand.RaiseCanExecuteChanged();
			}
		}

		public Playlist Playlist
		{
			get => _playlist;
			private set
			{
				SetProperty(ref _playlist, value);
				RaisePropertyChanged("IsDataAvailable");
			}
		}

		public Video Video
		{
			get => _video;
			private set
			{
				SetProperty(ref _video, value);
				RaisePropertyChanged("IsDataAvailable");
			}
		}

		public Channel Channel
		{
			get => _channel;
			private set
			{
				SetProperty(ref _channel, value);
				RaisePropertyChanged("IsDataAvailable");
			}
		}

		public bool IsDataAvailable => Playlist != null;

		public double Progress
		{
			get => _progress;
			private set => SetProperty(ref _progress, value);
		}

		public bool IsProgressIndeterminate
		{
			get => _isProgressIndeterminate;
			private set => SetProperty(ref _isProgressIndeterminate, value);
		}
		#endregion

		// Commands
		public DelegateCommand GetDataCommand { get; }
		public DelegateCommand<string> DownloadSongCommand { get; }
		public DelegateCommand<string> DownloadVideoCommand { get; }

		private static readonly string TempDirectoryPath = Path.Combine(Directory.GetCurrentDirectory(), "Temp");
		private static readonly string OutputDirectoryPath = Path.Combine(Directory.GetCurrentDirectory(), "Output");
		#endregion

		public MainViewModel()
		{
			MyTitle = "YouTube Toolerino";
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
			GetDataCommand = new DelegateCommand(GetData, () => !IsBusy && Query.IsNotBlank());
			DownloadSongCommand = new DelegateCommand<string>(o => DownloadSong(o), (o) => !IsBusy);
			DownloadVideoCommand = new DelegateCommand<string>(o => DownloadVideo(o), (o) => !IsBusy);
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

		#region YouTube Dling
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

		private static MediaStreamInfo GetBestAudioStreamInfo(MediaStreamInfoSet set)
		{
			if (set.Audio.Any())
				return set.Audio.WithHighestBitrate();
			if (set.Muxed.Any())
				return set.Muxed.WithHighestVideoQuality();
			throw new Exception("No applicable media streams found for this video");
		}
		#endregion

		#region Commands
		private async void DownloadSong(string o)
		{
			Video video = Playlist.Videos.First(v => v.Id == o);
			await DownloadSongAsync(video.Id);

			Status = "Ready";
		}

		private async void DownloadVideo(string o)
		{
			Video video = Playlist.Videos.First(v => v.Id == o);
			await DownloadVideoAsync(video.Id);

			Status = "Ready";
		}
		#endregion
	}
}