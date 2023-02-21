using CommandLine;

namespace unbooru.ImageSaveHandler
{
    [Verb("ImageSaveHandler")]
    public class ImageSaveCLIModel
    {
        [Option('S')] public bool Sync { get; set; }
    }
}
