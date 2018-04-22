using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YouTubeTool.ViewModels
{
	public interface ISettingsViewModel
	{
		bool IsAutoUpdateEnabled { get; set; }

		string DateFormat { get; set; }
	}
}