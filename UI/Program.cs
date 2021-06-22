using System;
using System.IO;
using System.Runtime.InteropServices;
using Avalonia;
using CefGlue.Avalonia;
using ImageInfrastructure.Abstractions;
using ImageInfrastructure.Core;

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

            AppBuilder.Configure<App>().UsePlatformDetect().UseSkia().ConfigureCefGlue(args).Start<MainWindow>();
        }

        private static bool IsRunningOnLinuxOrMac() => !RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    }
}
