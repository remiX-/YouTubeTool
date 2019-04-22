using System.IO;
using Tyrrrz.Settings;
using YouTubeTool.Extensions;

namespace YouTubeTool.Services
{
	public class PathService : IPathService
	{
		public string TempDirectoryPath { get; }

		public string OutputDirectoryPath { get; }

		public string LogFile { get; }

		public PathService(ISettingsService settingsService)
		{
			OutputDirectoryPath = Path.Combine(settingsService.OutputFolder.NullIfBlank() ?? Directory.GetCurrentDirectory(), "output");
			TempDirectoryPath = Path.Combine(settingsService.OutputFolder.NullIfBlank() ?? Directory.GetCurrentDirectory(), "temp");

			LogFile = Path.Combine(StorageSpace.SyncedUserDomain.GetDirectoryPath(), ".YouTubeTool", "log");
		}
	}
}