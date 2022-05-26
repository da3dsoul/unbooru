using CommandLine;

namespace unbooru.DeepDanbooru
{
    [Verb("Tag")]
    public class CLIModel
    {
        [Option] public int ID { get; set; }
        
    }
}
