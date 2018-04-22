using System.Collections.Generic;

namespace YouTubeTool.Services
{
    public interface ISettingsService
    {
		Dictionary<string, LayoutSettings> Windows { get; set; }

		bool IsAutoUpdateEnabled { get; set; }

        string DateFormat { get; set; }

        void Load();
        void Save();
    }
}