using CommandLine;

namespace unbooru.FoldSaveHandler
{
    [Verb("FoldSaveHandler")]
    public class FoldCLIModel
    {
        [Option('S')] public bool Sync { get; set; }
    }
}
