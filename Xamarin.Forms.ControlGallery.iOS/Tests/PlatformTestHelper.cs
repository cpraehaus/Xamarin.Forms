using System;
using System.Linq;
using System.Threading.Tasks;
using Foundation;
using SafariServices;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.ControlGallery.iOS.Tests;
using Xamarin.Forms.Controls.Issues.Helpers;

[assembly: Dependency(typeof(PlatformTestHelper))]
namespace Xamarin.Forms.ControlGallery.iOS.Tests
{
	[Internals.Preserve(AllMembers = true)]
	public class PlatformTestHelper : IPlatformTestHelper
	{
		class ViewControllerDisposable : IDisposable
		{
			UIViewController _viewController;
			public ViewControllerDisposable(UIViewController viewController) => (_viewController) = (viewController);

			public void Dispose()
			{
				_viewController?.DismissViewController(false, null);
				_viewController = null;
			}
		}

		public PlatformTestHelper()
		{

		}

		static UIViewController GetCurrentViewController(bool throwIfNull = true)
		{
			//var viewController = getCurrentController?.Invoke();

			//if (viewController != null)
			//	return viewController;
			UIViewController viewController = null;
			var window = UIApplication.SharedApplication.KeyWindow;

			if (window != null && window.WindowLevel == UIWindowLevel.Normal)
				viewController = window.RootViewController;

			if (viewController == null)
			{
				window = UIApplication.SharedApplication
					.Windows
					.OrderByDescending(w => w.WindowLevel)
					.FirstOrDefault(w => w.RootViewController != null && w.WindowLevel == UIWindowLevel.Normal);

				if (window == null && throwIfNull)
					throw new InvalidOperationException("Could not find current view controller.");
				else
					viewController = window?.RootViewController;
			}

			while (viewController?.PresentedViewController != null)
				viewController = viewController.PresentedViewController;

			if (throwIfNull && viewController == null)
				throw new InvalidOperationException("Could not find current view controller.");

			return viewController;
		}

		/// <remarks>Copied from Xamarin.Essentials Browser.PlatformOpenAsync() for testing </remarks>
		public async Task<IDisposable> PlatformOpenAsync(Uri uri)
		{
			var nativeUrl = new NSUrl(uri.AbsoluteUri);
			var sfViewController = new SFSafariViewController(nativeUrl, false);
			var vc = GetCurrentViewController();

			//if (options.PreferredToolbarColor.HasValue)
			//	sfViewController.PreferredBarTintColor = options.PreferredToolbarColor.Value.ToPlatformColor();

			//if (options.PreferredControlColor.HasValue)
			//	sfViewController.PreferredControlTintColor = options.PreferredControlColor.Value.ToPlatformColor();

			if (sfViewController.PopoverPresentationController != null)
				sfViewController.PopoverPresentationController.SourceView = vc.View;

			//if (options.HasFlag(BrowserLaunchFlags.PresentAsFormSheet))
			//	sfViewController.ModalPresentationStyle = UIModalPresentationStyle.FormSheet;
			//else if (options.HasFlag(BrowserLaunchFlags.PresentAsPageSheet))
			//	sfViewController.ModalPresentationStyle = UIModalPresentationStyle.PageSheet;

			await vc.PresentViewControllerAsync(sfViewController, true);

			return new ViewControllerDisposable(sfViewController);
		}
	}
}