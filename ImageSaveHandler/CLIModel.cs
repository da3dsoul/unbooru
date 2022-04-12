using CommandLine;

namespace unbooru.ImageSaveHandler
{
    public class CLIModel
    {
        [Option] public bool Sync { get; set; }
    }
}
