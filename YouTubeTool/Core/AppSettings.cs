using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YouTubeTool.Core
{
	public class AppSettings
	{
		#region Variables
		// Layouts
		public Dictionary<string, LayoutSettings> Windows { get; set; }
		#endregion

		public AppSettings() { }

		public AppSettings(bool loadDefaults)
		{
			if (loadDefaults)
			{
				LoadDefaults();
			}
		}

		public void Save()
		{
			string jsonFormatted = JToken.Parse(JsonConvert.SerializeObject(this)).ToString(Formatting.Indented);
			File.WriteAllText(AppGlobal.Paths.SettingsFile, jsonFormatted);
		}

		public static AppSettings Read()
		{
			AppSettings settings;
			try
			{
				string jsonData = File.ReadAllText(AppGlobal.Paths.SettingsFile);
				settings = JsonConvert.DeserializeObject<AppSettings>(jsonData);
			}
			catch (Exception)
			{
				settings = new AppSettings(true);
			}

			return settings;
		}

		private void LoadDefaults()
		{
			Windows = new Dictionary<string, LayoutSettings>
			{
				["Main"] = new LayoutSettings(1024, 576, false)
			};

			Save();
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
