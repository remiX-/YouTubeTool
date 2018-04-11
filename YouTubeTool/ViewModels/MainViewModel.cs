using MaterialDesignThemes.Wpf;
using Prism.Commands;
using Prism.Mvvm;
using Tyrrrz.Extensions;
using YoutubeExplode;
using YoutubeExplode.Models;
using YoutubeExplode.Models.ClosedCaptions;
using YoutubeExplode.Models.MediaStreams;

namespace YouTubeTool.ViewModels
{
	internal class MainViewModel : BindableBase
	{
		#region Variables
		public HamburgerMenuItem[] AppMenu { get; }
		private readonly YoutubeClient _client;

		#region Fields
		private string myTitle;

		private bool _isBusy;
		private string _query;
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


		public bool IsBusy
		{
			get => _isBusy;
			private set
			{
				SetProperty(ref _isBusy, value);
				GetDataCommand.RaiseCanExecuteChanged();
				//DownloadMediaStreamCommand.RaiseCanExecuteChanged();
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

		public Video Video
		{
			get => _video;
			private set
			{
				SetProperty(ref _video, value);
				//RaisePropertyChanged(() => IsDataAvailable);
			}
		}

		public Channel Channel
		{
			get => _channel;
			private set
			{
				SetProperty(ref _channel, value);
				//RaisePropertyChanged(() => IsDataAvailable);
			}
		}

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
		//public RelayCommand<MediaStreamInfo> DownloadMediaStreamCommand { get; }
		//public RelayCommand<ClosedCaptionTrackInfo> DownloadClosedCaptionTrackCommand { get; }
		#endregion

		public MainViewModel()
		{
			MyTitle = "YouTube Toolerino";

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
		}

		private async void GetData()
		{
			IsBusy = true;
			IsProgressIndeterminate = true;

			// Reset data
			Video = null;
			Channel = null;
			//MediaStreamInfos = null;
			//ClosedCaptionTrackInfos = null;

			// Parse URL if necessary
			if (!YoutubeClient.TryParsePlaylistId(Query, out var playlistId))
				playlistId = Query;

			// Get data
			Playlist q = await _client.GetPlaylistAsync(playlistId);
			//Video = await _client.GetVideoAsync(videoId);
			//Channel = await _client.GetVideoAuthorChannelAsync(videoId);
			//MediaStreamInfos = await _client.GetVideoMediaStreamInfosAsync(videoId);
			//ClosedCaptionTrackInfos = await _client.GetVideoClosedCaptionTrackInfosAsync(videoId);

			IsBusy = false;
			IsProgressIndeterminate = false;
		}
	}
}