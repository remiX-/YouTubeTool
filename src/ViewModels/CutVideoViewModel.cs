using CliWrap;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Tyrrrz.Extensions;
using YouTubeTool.Services;
using static System.Environment;
using WinForms = System.Windows.Forms;

namespace YouTubeTool.ViewModels
{
	internal class CutVideoViewModel : ViewModelBase, ICutVideoViewModel
	{
		#region Vars
		private readonly ISettingsService _settingsService;
		private readonly IPathService _pathService;

		private readonly Cli FfmpegCli = new Cli("ffmpeg.exe");

		#region Fields
		bool isBusy;

		private string inputFile;
		private string outputFile;

		private bool timeSlice;
		private bool removeAudio;

		private TimeSpan startTime;
		private TimeSpan endTime;
		#endregion

		#region Properties
		public bool IsBusy
		{
			get => isBusy;
			private set
			{
				Set(ref isBusy, value);
				GoCommand.RaiseCanExecuteChanged();
			}
		}

		public bool HasManuallySelectedOuput { get; set; }

		public string InputFile
		{
			get => inputFile;
			set
			{
				Set(ref inputFile, value);
				GoCommand.RaiseCanExecuteChanged();
			}
		}

		public string OutputFile
		{
			get => outputFile;
			set => Set(ref outputFile, value);
		}

		public bool TimeSlice
		{
			get => timeSlice;
			set => Set(ref timeSlice, value);
		}

		public bool RemoveAudio
		{
			get => removeAudio;
			set => Set(ref removeAudio, value);
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
		#endregion

		#region Commands
		public RelayCommand BrowseInputFileCommand { get; }
		public RelayCommand BrowseOutputFileCommand { get; }

		public RelayCommand GoCommand { get; }
		#endregion
		#endregion

		#region View
		public CutVideoViewModel(ISettingsService settingsService, IPathService pathService)
		{
			_settingsService = settingsService;
			_pathService = pathService;

			OutputFile = _pathService.OutputDirectoryPath;

			BrowseInputFileCommand = new RelayCommand(BrowseInputFile);
			BrowseOutputFileCommand = new RelayCommand(BrowseOutputFile);

			GoCommand = new RelayCommand(Go, () => !IsBusy && InputFile.IsNotBlank() && File.Exists(InputFile) && OutputFile.IsNotBlank());
		}
		#endregion

		private async Task ProcessVideo()
		{
			IsBusy = true;

			// Setup
			var fileName = Path.GetFileNameWithoutExtension(InputFile);
			var extension = Path.GetExtension(InputFile).Replace(".", "");

			var cleanTitle = fileName.Replace(Path.GetInvalidFileNameChars(), '_');
			var outputTempFilePath = Path.Combine(_pathService.TempDirectoryPath, $"{Guid.NewGuid()}.{extension}");

			var argsInit = BuildInitialArgsString(outputTempFilePath);
			var argsFinal = BuildFinalArgsString(outputTempFilePath, OutputFile);

			// Process
			Directory.CreateDirectory(_pathService.TempDirectoryPath);
			Directory.CreateDirectory(_pathService.OutputDirectoryPath);
			var result1 = await FfmpegCli.SetArguments(argsInit).ExecuteAsync();
			var result2 = await FfmpegCli.SetArguments(argsFinal).ExecuteAsync();

			// Delete temp file
			File.Delete(outputTempFilePath);

			IsBusy = false;

			InputFile = String.Empty;
			OutputFile = String.Empty;
		}

		#region Commands
		private async void Go() => await ProcessVideo();

		private void BrowseInputFile()
		{
			WinForms.OpenFileDialog ofd = new WinForms.OpenFileDialog
			{
				Multiselect = false,
				InitialDirectory = Path.GetDirectoryName(InputFile) ?? GetFolderPath(SpecialFolder.MyVideos)
			};

			var q = GetLogicalDrives();

			if (ofd.ShowDialog() == WinForms.DialogResult.OK)
			{
				InputFile = ofd.FileName;

				if (!HasManuallySelectedOuput)
				{
					OutputFile = Path.Combine(_pathService.OutputDirectoryPath, Path.GetFileName(InputFile));
				}
			}
		}

		private void BrowseOutputFile()
		{
			WinForms.SaveFileDialog sfd = new WinForms.SaveFileDialog
			{
				Title = "Specify save location",
				InitialDirectory = Path.GetDirectoryName(OutputFile) ?? _settingsService.OutputFolder.NullIfBlank() ?? Directory.GetCurrentDirectory(),
				AddExtension = true,
				DefaultExt = "mp4"
			};

			if (sfd.ShowDialog() == WinForms.DialogResult.OK)
			{
				HasManuallySelectedOuput = true;
				OutputFile = sfd.FileName;
			}
		}
		#endregion

		private string BuildInitialArgsString(string output)
		{
			var duration = EndTime - StartTime;

			var args = new List<string>
			{
				$"-i \"{InputFile}\"",
				"-y"
			};

			if (TimeSlice)
			{
				args.Add($"-ss {StartTime}");
				args.Add($"-t {duration}");
			}

			args.Add("-c copy");
			args.Add($"\"{output}\"");

			return args.JoinToString(" ");
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