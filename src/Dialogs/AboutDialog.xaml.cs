using System.Windows.Controls;
using YouTubeTool.Core;

namespace YouTubeTool.Dialogs
{
	public partial class AboutDialog : UserControl
	{
		#region Variables
		public string ProductName { get; set; }
		public string Version { get; set; }
		public string Copyright { get; set; }
		public string CompanyName { get; set; }
		public string Description { get; set; }
		#endregion

		public AboutDialog()
		{
			InitializeComponent();

			ProductName = AppGlobal.AssemblyProduct;
			Version = $"v{AppGlobal.AssemblyVersion}";
			Copyright = AppGlobal.AssemblyCopyright;
			CompanyName = AppGlobal.AssemblyCompany;
			Description = AppGlobal.AssemblyDescription;

			DataContext = this;
		}
	}
}
