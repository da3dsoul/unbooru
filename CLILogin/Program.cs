using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using ImageInfrastructure.Abstractions;
using ImageInfrastructure.Abstractions.Interfaces;
using ImageInfrastructure.Core;
using ImageInfrastructure.Pixiv;
using Meowtrix.PixivApi;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace ImageInfrastructure.CLI
{
    public static class CliProcess
    {
        public static async Task Main(string[] args)
        {
            Arguments.DataPath = IsRunningOnLinuxOrMac()
                ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".unbooru")
                : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "unbooru");
            var start = new Startup();
            var host = start.CreateHostBuilder(args).Build();
            var client = new PixivClient();
            var token = await client.LoginAsync(uri =>
            {
                Console.WriteLine(uri);
                string url = null;
                while (url == null || !url.StartsWith("pixiv"))
                {
                    url = Console.ReadLine().Trim();
                }
                var result = new Uri(url);
                return Task.FromResult(result);
            });

            var settingsProvider = host.Services?.GetService<ISettingsProvider<PixivSettings>>();
            settingsProvider?.Update(a => { a.Token = token; });
        }

        private static bool IsRunningOnLinuxOrMac() => !RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    }
}