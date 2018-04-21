using Onova;
using Onova.Services;
using System;
using System.Threading.Tasks;

namespace YouTubeTool.Services
{
	public class UpdateService : IUpdateService
	{
		private readonly IUpdateManager _manager;

		private Version _updateVersion;
		private bool _updateFinalized;

		public bool NeedRestart { get; set; }

		public UpdateService()
		{
			_manager = new UpdateManager(new GithubPackageResolver("remiX-", "YouTube-tool", "YouTubeTool.zip"), new ZipPackageExtractor());
		}

		public async Task<Version> CheckPrepareUpdateAsync()
		{
#if DEBUG
			// Never update in DEBUG mode
			//return null;
#endif

			// Cleanup leftover files
			_manager.Cleanup();

			// Check for updates
			var check = await _manager.CheckForUpdatesAsync();
			if (!check.CanUpdate)
				return null;

			// Prepare the update
			if (!_manager.IsUpdatePrepared(check.LastVersion))
				await _manager.PrepareUpdateAsync(check.LastVersion);

			return _updateVersion = check.LastVersion;
		}

		public void FinalizeUpdate()
		{
			// Check if an update is pending
			if (_updateVersion == null)
				return;

			// Check if the update has already been finalized
			if (_updateFinalized)
				return;

			// Launch the updater
			_manager.LaunchUpdater(_updateVersion, NeedRestart);
			_updateFinalized = true;
		}
	}
}
