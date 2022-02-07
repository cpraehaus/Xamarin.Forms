using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Controls.Issues.Helpers;


#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.Navigation)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.None, 220207, "[Bug] PushModalAsync fails on iOS if another UIViewController is active (e.g. through XE Browser.OpenAsync)",
		PlatformAffected.iOS)]
	public class IssueN220207 : TestNavigationPage
	{
		protected override void Init()
		{
#if APP
			PushAsync(CreateContentPage());
#endif
		}

		ContentPage CreatePage(string title, bool isModal, string titleId = "")
		{
			StackLayout layout = new StackLayout();
			title += isModal ? " (modal)" : "(non-modal)";
			var titleLabel = new Label()
			{
				Text = title,
				AutomationId = titleId,
				VerticalOptions = LayoutOptions.CenterAndExpand,
				HorizontalTextAlignment = TextAlignment.Center
			};

			var popButton = new Button()
			{
				Text = "Go back",
				Command = new Command(async () =>
				{
					if (isModal)
					{
						await Navigation.PopModalAsync();
					}
					else
					{
						await Navigation.PopAsync();
					}
				})
			};

			layout.Children.Add(titleLabel);
			layout.Children.Add(popButton);

			return new ContentPage()
			{
				Title = title,
				Content = layout
			};
		}

		ContentPage CreateContentPage()
		{
			StackLayout layout = new StackLayout();

			var platformHelper = DependencyService.Get<IPlatformTestHelper>();

			// Opens browser SFSafariViewController using IPlatformTestHelper similar to Xamarin.Essentials Browser.PlatformOpenAsync()
			// and attempts to open modal page a few seconds afterwards
			Button pushButton = new Button()
			{
				Text = "Open browser & push modal page B",
				AutomationId = "PushButton",
				Command = new Command(async () =>
				{
					Debug.WriteLine($"Call PlatformOpenAsync ...");
					Task pushModalTask;
					using (await platformHelper.PlatformOpenAsync(new Uri("https://www.xamarin.com/")))
					{
						Debug.WriteLine($"PlatformOpenAsync done");
						await Task.Delay(TimeSpan.FromSeconds(3));
						Debug.WriteLine($"PushModalAsync start");
						// This produces the following warning/error after push 
						// [0:] PushModalAsync ...
						// [0:] Xamarin.Forms.ContentPage is replaced by _2489CustomRenderer
						// 2022 - 02 - 07 14:46:06.587 XamarinFormsControlGalleryiOS[448:286809] Warning:
						// Attempt to present < Xamarin_Forms_Platform_iOS_ModalWrapper: 0x14dd379b0 > on < Xamarin_Forms_Platform_iOS_NavigationRenderer: 0x14eaaa000 > whose view is not in the window hierarchy!
						pushModalTask = Navigation.PushModalAsync(CreatePage("Page B", true, "ModalTitle"), false);
						await Task.Delay(TimeSpan.FromSeconds(2));
					}
					Debug.WriteLine($"Exit PlatformOpenAsync");
					await Task.Delay(TimeSpan.FromSeconds(2));
					await pushModalTask;
					Debug.WriteLine($"PushModalAsync done");
				})
			};

			Button pushButton2 = new Button()
			{
				Text = "Push page C (modal)",
				Command = new Command(async () =>
				{
					await Navigation.PushModalAsync(CreatePage("Page C", true));
				})
			};

			Label titleLabel = new Label()
			{
				Text = $"Page A (root)",
				VerticalOptions = LayoutOptions.CenterAndExpand,
				HorizontalTextAlignment = TextAlignment.Center
			};

			layout.Children.Add(titleLabel);
			layout.Children.Add(pushButton);
			layout.Children.Add(pushButton2);

			return new ContentPage()
			{
				Title = "Page A (root)",
				Content = layout
			};
		}

#if UITEST && __IOS__
		[Test]
		public void PushModalPageWhileExternalViewControllerIsVisible()
		{
			RunningApp.Tap("PushButton");
			
			RunningApp.WaitForElement("ModalTitle", timeout: TimeSpan.FromSeconds(15));
		}
#endif
	}
}
