using CliWrap;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System;
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
			WinForms.OpenFileDialog fbd = new WinForms.OpenFileDialog
			{
				Multiselect = false,
				InitialDirectory = _settingsService.OutputFolder.NullIfBlank() ?? Directory.GetCurrentDirectory()
			};

			if (fbd.ShowDialog() == WinForms.DialogResult.OK)
			{
				InputFile = fbd.FileName;
			}
		}

		private async void Go()
		{
			var fileName = Path.GetFileNameWithoutExtension(InputFile);
			var extension = Path.GetExtension(InputFile).Replace(".", "");

			var cleanTitle = fileName.Replace(Path.GetInvalidFileNameChars(), '_');
			var outputFilePath = Path.Combine(OutputDirectoryPath, $"{cleanTitle}");

			var duration = EndTime - StartTime;

			var args1 = new string[]
			{
				$"-i \"{InputFile}\"",
				$"-ss {StartTime}",
				$"-t {duration}",
				"-c copy",
				$"\"{outputFilePath}_temp1.{extension}\""
			};
			var args2 = new string[]
			{
				$"-i \"{outputFilePath}_1.{extension}\"",
				"-vcodec h264",
				$"\"{outputFilePath}_temp2.{extension}\""
			};
			var args3 = new string[]
			{
				$"-i \"{outputFilePath}_2.{extension}\"",
				"-b 8507k",
				$"\"{outputFilePath}_final.{extension}\""
			};

			MessengerInstance.Send(new ShowNotificationMessage("Starting ..."));

			var q = await FfmpegCli.ExecuteAsync(args1.JoinToString(" "));
			await FfmpegCli.ExecuteAsync(args2.JoinToString(" "));
			await FfmpegCli.ExecuteAsync(args3.JoinToString(" "));

			MessengerInstance.Send(new ShowNotificationMessage("Done !"));
		}
	}
}