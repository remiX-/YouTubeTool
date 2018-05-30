using CliWrap;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.IO;
using Tyrrrz.Extensions;
using YouTubeTool.Services;
using YouTubeTool.Utils.Messages;
using WinForms = System.Windows.Forms;

namespace YouTubeTool.ViewModels
{
	internal class CutVideoViewModel : ViewModelBase, ICutVideoViewModel
	{
		#region Vars
		private readonly ISettingsService _settingsService;
		private readonly IPathService _pathService;

		private readonly Cli FfmpegCli = new Cli("ffmpeg.exe");

		private string inputFile;
		private string outputFile;

		private TimeSpan startTime;
		private TimeSpan endTime;

		private bool removeAudio;

		public string InputFile
		{
			get => inputFile;
			set => Set(ref inputFile, value);
		}

		public string OutputFile
		{
			get => outputFile;
			set => Set(ref outputFile, value);
		}

		public TimeSpan StartTime
		{
			get => startTime;
			set => Set(ref startTime, value);
		}

		public TimeSpan EndTime
		{
			get => endTime;
			set => Set(ref endTime, value);
		}

		public bool RemoveAudio
		{
			get => removeAudio;
			set => Set(ref removeAudio, value);
		}

		public RelayCommand BrowseInputFileCommand { get; }
		public RelayCommand BrowseOutputFileCommand { get; }

		public RelayCommand GoCommand { get; }

		public RelayCommand ViewLoadedCommand { get; }
		#endregion

		public CutVideoViewModel(ISettingsService settingsService, IPathService pathService)
		{
			_settingsService = settingsService;
			_pathService = pathService;

			BrowseInputFileCommand = new RelayCommand(BrowseInputFile);
			BrowseOutputFileCommand = new RelayCommand(BrowseOutputFile);

			GoCommand = new RelayCommand(Go);
			
			// Window Events
			ViewLoadedCommand = new RelayCommand(ViewLoaded);
		}

		private void ViewLoaded()
		{
			OutputFile = _pathService.OutputDirectoryPath;
		}

		private void BrowseInputFile()
		{
			WinForms.OpenFileDialog ofd = new WinForms.OpenFileDialog
			{
				Multiselect = false,
				InitialDirectory = Directory.GetCurrentDirectory()
			};
			ofd.InitialDirectory = @"C:\Users\remiX\Videos\nVidia Share\Rocket League";

			if (ofd.ShowDialog() == WinForms.DialogResult.OK)
			{
				InputFile = ofd.FileName;
				OutputFile = Path.Combine(_pathService.OutputDirectoryPath, Path.GetFileName(InputFile));
			}
		}

		private void BrowseOutputFile()
		{
			WinForms.SaveFileDialog sfd = new WinForms.SaveFileDialog
			{
				InitialDirectory = _settingsService.OutputFolder.NullIfBlank() ?? Directory.GetCurrentDirectory()
			};

			if (sfd.ShowDialog() == WinForms.DialogResult.OK)
			{
				OutputFile = sfd.FileName;
			}
		}

		private async void Go()
		{
			var fileName = Path.GetFileNameWithoutExtension(InputFile);
			var extension = Path.GetExtension(InputFile).Replace(".", "");


			var cleanTitle = fileName.Replace(Path.GetInvalidFileNameChars(), '_');
			var outputTempFilePath = Path.Combine(_pathService.OutputDirectoryPath, $"{cleanTitle}_temp.{extension}");

			var outputFinalFilePath = Path.Combine(_pathService.OutputDirectoryPath, $"{cleanTitle}_final.{extension}");

			var duration = EndTime - StartTime;

			var args1 = new string[]
			{
				$"-i \"{InputFile}\"",
				$"-ss {StartTime}",
				$"-t {duration}",
				"-c copy",
				$"\"{outputTempFilePath}\""
			};
			var argsFinal = BuildFinalArgsString($"{outputTempFilePath}", $"{outputFinalFilePath}");

			MessengerInstance.Send(new ShowNotificationMessage("Starting ..."));

			await FfmpegCli.ExecuteAsync(args1.JoinToString(" "));
			await FfmpegCli.ExecuteAsync(argsFinal);

			MessengerInstance.Send(new ShowNotificationMessage("Done !"));
		}

		private string BuildFinalArgsString(string input, string output)
		{
			var args = new List<string>
			{
				$"-i \"{input}\"",
				"-y"
			};

			if (RemoveAudio) args.Add("-an");
			args.Add("-r 30");
			args.Add("-s 1280x720");
			args.Add("-c:v libx264");
			args.Add("-b:v 3M");
			args.Add($"\"{output}\"");

			return args.JoinToString(" ");
		}
	}
}