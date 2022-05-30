using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.ML;
using unbooru.Abstractions;

namespace unbooru.DeepDanbooru;

public class Trainer
{
    private readonly ILogger<Trainer> _logger;
    private readonly MLContext _mlContext;
    private readonly string[] _tags;
    
    public Trainer(ILogger<Trainer> logger)
    {
        _logger = logger;
        _mlContext = new MLContext(seed: 0);

        var tagsLocation = Path.Combine(Arguments.DataPath, "DeepDanbooru", "tags.txt");
        if (!File.Exists(tagsLocation)) return;
        _tags = File.ReadAllLines(tagsLocation);
    }
    
}