using CommonServiceLocator;
using GalaSoft.MvvmLight.Ioc;
using YouTubeTool.ViewModels;

namespace YouTubeTool.Core
{
	public class Locator
	{
		public IMainViewModel MainViewModel => ServiceLocator.Current.GetInstance<IMainViewModel>();

		public static void Init()
		{
			ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

			SimpleIoc.Default.Register<IMainViewModel, MainViewModel>();
		}

		public static void Cleanup()
		{

		}
	}
}
