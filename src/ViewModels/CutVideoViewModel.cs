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

		private readonly Cli FfmpegCli = new Cli("ffmpeg.exe");

		private string OutputDirectoryPath => Path.Combine(_settingsService.OutputFolder.NullIfBlank() ?? Directory.GetCurrentDirectory(), "output");

		private string inputFile;

		private TimeSpan startTime;
		private TimeSpan endTime;

		private bool removeAudio;

		public string InputFile
		{
			get => inputFile;
			set => Set(ref inputFile, value);
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

		public RelayCommand GoCommand { get; }
		#endregion

		public CutVideoViewModel(ISettingsService settingsService)
		{
			_settingsService = settingsService;

			BrowseInputFileCommand = new RelayCommand(BrowseInputFolder);
			GoCommand = new RelayCommand(Go);
		}

		private void BrowseInputFolder()
		{
			WinForms.OpenFileDialog ofd = new WinForms.OpenFileDialog
			{
				Multiselect = false,
				InitialDirectory = _settingsService.OutputFolder.NullIfBlank() ?? Directory.GetCurrentDirectory()
			};
			ofd.InitialDirectory = @"C:\Users\remiX\Videos\nVidia Share\Rocket League";

			if (ofd.ShowDialog() == WinForms.DialogResult.OK)
			{
				InputFile = ofd.FileName;
			}
		}

		private async void Go()
		{
			var fileName = Path.GetFileNameWithoutExtension(InputFile);
			var extension = Path.GetExtension(InputFile).Replace(".", "");

			var cleanTitle = fileName.Replace(Path.GetInvalidFileNameChars(), '_');
			var outputTempFilePath = Path.Combine(OutputDirectoryPath, $"{cleanTitle}_temp.{extension}");
			var outputFinalFilePath = Path.Combine(OutputDirectoryPath, $"{cleanTitle}_final.{extension}");

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