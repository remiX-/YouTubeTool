using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YouTubeTool.Services
{
	public interface IUpdateService
	{
		Task<Version> CheckForUpdatesAsync();

		Task ApplyUpdate();

		Task PrepareUpdateAsync();

		void ApplyUpdateAsync(bool restart = true);
	}
}
