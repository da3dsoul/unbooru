using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using ImageInfrastructure.Abstractions;
using ImageInfrastructure.Core;

namespace ImageInfrastructure.CLI
{
    public static class CliProcess
    {
        public static Task Main(string[] args)
        {
            Arguments.DataPath = IsRunningOnLinuxOrMac()
                ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".unbooru")
                : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "unbooru");
            return new Startup().Main(args);
        }
        
        public static bool IsRunningOnLinuxOrMac() => !RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    }
}