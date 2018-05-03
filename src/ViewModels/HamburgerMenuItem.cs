using GalaSoft.MvvmLight;
using MaterialDesignThemes.Wpf;

namespace YouTubeTool.ViewModels
{
	public class HamburgerMenuItem : ViewModelBase
	{
		private string id;
		private string description;
		private PackIconKind icon;

		private object content;

		public string Id
		{
			get => id;
			set => Set(ref id, value);
		}

		public string Description
		{
			get => description;
			set => Set(ref description, value);
		}

		public PackIconKind Icon
		{
			get => icon;
			set => Set(ref icon, value);
		}


		public object Content
		{
			get => content;
			set => Set(ref content, value);
		}

		public HamburgerMenuItem(string id, string description, PackIconKind icon)
		{
			this.id = id;
			this.description = description;
			this.icon = icon;
		}

		public HamburgerMenuItem(string id, PackIconKind icon)
		{
			this.id = id;
			this.description = id;
			this.icon = icon;
		}

		public HamburgerMenuItem(string id, string name, PackIconKind icon, object content) : this(id, name, icon)
		{
			this.content = content;
		}
	}
}