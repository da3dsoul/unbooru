using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.ReactiveUI;
using ImageInfrastructure.Abstractions;
using ImageInfrastructure.Core;
using Microsoft.Extensions.DependencyInjection;

namespace UI
{
    class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        public static IServiceProvider? ServiceProvider;
        
        public static void Main(string[] args)
        {
            Arguments.DataPath = IsRunningOnLinuxOrMac()
                ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".unbooru")
                : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "unbooru");
            var startup = new Startup();
            var host = startup.CreateHostBuilder(args).Build();
            ServiceProvider = host.Services;

            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        private static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace()
                .UseReactiveUI();

        private static bool IsRunningOnLinuxOrMac() => !RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    }
}
