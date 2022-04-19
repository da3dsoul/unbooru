using CommandLine;

namespace unbooru.ImageSaveHandler
{
    [Verb("ImageSaveHandler")]
    public class CLIModel
    {
        [Option] public bool Sync { get; set; }
    }
}
