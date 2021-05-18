using System.Threading.Tasks;
using ImageInfrastructure.Core;

namespace ImageInfrastructure.CLI
{
    public static class CliProcess
    {
        public static Task Main(string[] args)
        {
            return new Startup().Main(args);
        }
    }
}