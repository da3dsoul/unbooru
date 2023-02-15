using CommandLine;

namespace unbooru.ImageSaveHandler
{
    [Verb("ImageSaveHandler")]
    public class CLIModel
    {
        [Option('S')] public bool Sync { get; set; }
    }
}
