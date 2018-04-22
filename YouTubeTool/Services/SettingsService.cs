using System.Collections.Generic;
using Tyrrrz.Settings;

namespace YouTubeTool.Services
{
	public class SettingsService : SettingsManager, ISettingsService
	{
		// Layouts
		public Dictionary<string, LayoutSettings> Windows { get; set; }

		public bool IsAutoUpdateEnabled { get; set; }

		public string DateFormat { get; set; }

		public SettingsService()
		{
			IsSaved = false;

			Configuration.StorageSpace = StorageSpace.SyncedUserDomain;
			Configuration.SubDirectoryPath = ".YouTubeTool";
			Configuration.FileName = "config";
			Configuration.ThrowIfCannotLoad = true;
			Configuration.ThrowIfCannotSave = true;
		}

		public override void Load()
		{
			base.Load();

			if (!IsSaved)
			{
				LoadDefaults();
				Save();
			}
		}

		private void LoadDefaults()
		{
			// Layout
			Windows = new Dictionary<string, LayoutSettings>
			{
				["Main"] = new LayoutSettings(1024, 576, false)
			};

			IsAutoUpdateEnabled = true;

			DateFormat = "dd-MMM-yy hh:mm tt";
		}
	}

	public class LayoutSettings
	{
		public double Width { get; set; }
		public double Height { get; set; }

		public bool Maximized { get; set; }

		public LayoutSettings(double width, double height, bool maximized)
		{
			Width = width;
			Height = height;
			Maximized = maximized;
		}
	}
}