using System;
using Android.App;
using Android.Runtime;
using Jellyfin.Sdk;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;

namespace Jellyfin.Maui
{
	[Application]
	public class MainApplication : MauiApplication
	{
		public MainApplication(IntPtr handle, JniHandleOwnership ownership)
			: base(handle, ownership)
		{
			var version = typeof(MauiProgram).Assembly.GetName().Version?.ToString() ?? "0.0.0.1";
			this.Services.GetRequiredService<SdkClientSettings>()
			   .InitializeClientSettings(
			   "Jellyfin Maui Android",
			   version,
			   "Android",
			   Guid.NewGuid().ToString("N"));
		}

		protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
	}
}