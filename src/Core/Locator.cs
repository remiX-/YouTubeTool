using CommonServiceLocator;
using GalaSoft.MvvmLight.Ioc;
using YouTubeTool.Services;
using YouTubeTool.ViewModels;

namespace YouTubeTool.Core
{
	public class Locator
	{
		public IMainViewModel MainViewModel => Resolve<IMainViewModel>();
		public ISettingsViewModel SettingsViewModel => Resolve<ISettingsViewModel>();
		public ICutVideoViewModel CutVideoViewModel => Resolve<ICutVideoViewModel>();

		private T Resolve<T>(string key = null)
		{
			return ServiceLocator.Current.GetInstance<T>(key);
		}

		public static void Init()
		{
			ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);
			SimpleIoc.Default.Reset();

			// Services
			SimpleIoc.Default.Register<ISettingsService, SettingsService>();
			SimpleIoc.Default.Register<IUpdateService, UpdateService>();

			// View models
			SimpleIoc.Default.Register<IMainViewModel, MainViewModel>(true);
			SimpleIoc.Default.Register<ISettingsViewModel, SettingsViewModel>(true);
			SimpleIoc.Default.Register<ICutVideoViewModel, CutVideoViewModel>(true);
		}

		public static void Cleanup()
		{

		}
	}
}
