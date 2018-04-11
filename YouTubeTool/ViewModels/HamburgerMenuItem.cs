using MaterialDesignThemes.Wpf;
using Prism.Mvvm;

namespace YouTubeTool.ViewModels
{
	public class HamburgerMenuItem : BindableBase
	{
		private string id;
		private string description;
		private PackIconKind icon;

		private object content;

		public string Id
		{
			get => id;
			set => SetProperty(ref id, value);
		}

		public string Description
		{
			get => description;
			set => SetProperty(ref description, value);
		}

		public PackIconKind Icon
		{
			get => icon;
			set => SetProperty(ref icon, value);
		}


		public object Content
		{
			get => content;
			set => SetProperty(ref content, value);
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