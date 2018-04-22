using System.Collections.Generic;

namespace YouTubeTool.Services
{
    public interface ISettingsService
    {
		LayoutSettings WindowSettings { get; set; }

		bool IsAutoUpdateEnabled { get; set; }

        string DateFormat { get; set; }

        void Load();
        void Save();
    }
}