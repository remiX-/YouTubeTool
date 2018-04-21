using Onova;
using Onova.Services;
using System;
using System.Threading.Tasks;

namespace YouTubeTool.Services
{
	public class UpdateService : IUpdateService
	{
		private readonly UpdateManager _updateManager;

		private Version _lastVersion;
		private bool _applied;

		public UpdateService()
		{
			_updateManager = new UpdateManager(new GithubPackageResolver("remiX-", "YouTube-tool", "youtube.tool.zip"), new ZipPackageExtractor());
		}

		public async Task<Version> CheckForUpdatesAsync()
		{
#if DEBUG
			// Never update in DEBUG mode
			return null;
#endif

			// Remove some junk left over from last update
			_updateManager.Cleanup();

			// Check for updates
			var check = await _updateManager.CheckForUpdatesAsync();

			// Return latest version or null if running latest version already
			return check.CanUpdate ? _lastVersion = check.LastVersion : null;
		}

		public async Task ApplyUpdate()
		{
			// Prepare an update so it can be applied later
			// (supports optional progress reporting and cancellation)
			await _updateManager.PrepareUpdateAsync(_lastVersion);

			// Launch an executable that will apply the update
			// (can optionally restart application on completion)
			_updateManager.LaunchUpdater(_lastVersion);

			// External updater will wait until the application exits
			Environment.Exit(0);
		}

		public async Task PrepareUpdateAsync()
		{
			if (_lastVersion == null)
				return;

			// Download and prepare update
			await _updateManager.PrepareUpdateAsync(_lastVersion);
		}

		public void ApplyUpdateAsync(bool restart = true)
		{
			if (_lastVersion == null)
				return;
			if (_applied)
				return;

			// Enqueue an update
			_updateManager.LaunchUpdater(_lastVersion, restart);

			_applied = true;
		}
	}
}
