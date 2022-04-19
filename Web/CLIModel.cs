using CommandLine;

namespace unbooru.Web
{
    [Verb("web")]
    public class CLIModel
    {
        [Option] public bool WebOnly { get; set; }
    }
}
