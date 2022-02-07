using System;
using System.Threading.Tasks;

namespace Xamarin.Forms.Controls.Issues.Helpers
{
	/// <summary>
	/// Platform-specific helper functions for use in unit test
	/// </summary>
	public interface IPlatformTestHelper
	{
		Task<IDisposable> PlatformOpenAsync(Uri uri);
	}
}
