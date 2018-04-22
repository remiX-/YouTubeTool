using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YouTubeTool.Services
{
	public interface IUpdateService
	{
		bool NeedRestart { get; set; }

		Task<Version> CheckForUpdateAsync();
		Task PrepareUpdateAsync();

		Task<Version> CheckPrepareUpdateAsync();

		void FinalizeUpdate();
	}
}
