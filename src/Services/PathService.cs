using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tyrrrz.Extensions;
using Tyrrrz.Settings;

namespace YouTubeTool.Services
{
	public class PathService : IPathService
	{
		public string TempDirectoryPath { get; }

		public string OutputDirectoryPath { get; }

		public string LogFile { get; }

		public PathService(ISettingsService settingsService)
		{
			TempDirectoryPath = Path.Combine(settingsService.OutputFolder.NullIfBlank() ?? Directory.GetCurrentDirectory(), "output");
			OutputDirectoryPath = Path.Combine(settingsService.OutputFolder.NullIfBlank() ?? Directory.GetCurrentDirectory(), "temp");

			LogFile = Path.Combine(StorageSpace.SyncedUserDomain.GetDirectoryPath(), ".YouTubeTool", "log");
		}
	}
}