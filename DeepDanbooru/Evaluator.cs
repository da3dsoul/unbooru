using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ImageMagick;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.ML;
using Microsoft.ML.Runtime;
using Microsoft.ML.Transforms.Onnx;
using unbooru.Abstractions;
using unbooru.Abstractions.Interfaces;
using unbooru.Abstractions.Poco;
using unbooru.Abstractions.Poco.Events;

namespace unbooru.DeepDanbooru;

public class Evaluator
{
    private readonly ILogger<Evaluator> _logger;
    private readonly MLContext _mlContext;
    private readonly ITransformer _transformer;
    private readonly string[] _tags;
    
    public Evaluator(ILogger<Evaluator> logger)
    {
        _logger = logger;
        _mlContext = new MLContext(seed: 0);
        try
        {
            _transformer = GetBasicTransformer();
        }
        catch (Exception e)
        {
            logger.LogError(e, "{Message}", e.ToString());
        }

        var tagsLocation = Path.Combine(Arguments.DataPath, "DeepDanbooru", "tags.txt");
        if (!File.Exists(tagsLocation)) return;
        _tags = File.ReadAllLines(tagsLocation);
    }

    private ITransformer GetBasicTransformer()
    {
        _logger.LogInformation("Creating OnnxTransformer");
        var modelLocation = Path.Combine(Arguments.DataPath, "DeepDanbooru", "model-resnet-custom_v3.onnx");
        if (!File.Exists(modelLocation)) return null;
        // Define scoring pipeline
        var estimator = _mlContext.Transforms.ApplyOnnxModel(new OnnxOptions
        {
            InputColumns = new[] { ModelInput.InputString },
            OutputColumns = new[] { ModelOutput.OutputString },
            ModelFile = modelLocation,
            InterOpNumThreads = 10,
            IntraOpNumThreads = 10,
            GpuDeviceId = 0,
            FallbackToCpu = true
        });

        _mlContext.Log += (_, args) =>
        {
            if (args.Kind != ChannelMessageKind.Error) return;
            _logger.LogError("{Message}", args.Message);
        };
        var transformer = estimator.Fit(_mlContext.Data.LoadFromEnumerable(Array.Empty<ModelInput>()));
        _logger.LogInformation("Created OnnxTransformer");
        return transformer;
    }
    
    public Task FindTags(ImageProvidedEventArgs e)
    {
        return FindTags(e.ServiceProvider, e.Images, e.CancellationToken);
    }

    public async Task FindTags(IServiceProvider provider, List<Image> images, CancellationToken token)
    {
        if (token.IsCancellationRequested) return;
        if (_transformer == null || _tags == null) return;
        _logger.LogInformation("Searching the abyss for tags for: {Image}",
            string.Join(", ", images.Select(a => a.GetPixivFilename())));
        try
        {
            var sw = Stopwatch.StartNew();
            var processedImages = PredictMultipleImages(images);
            foreach (var (image, output) in processedImages)
            {
                var postTags = output.Select(a => new ImageTag
                {
                    Name = a.Replace("_", " "),
                    TagSources = new List<ImageTagSource>()
                }).ToList();

                sw.Restart();

                var tagContext = provider.GetRequiredService<IContext<ImageTag>>();
                tagContext.DisableLogging = true;
                var outputTags = await tagContext.Get(postTags, token: token);
                if (token.IsCancellationRequested) return;
                tagContext.DisableLogging = false;
                sw.Stop();
                _logger.LogInformation("Got tags from database in {Time}", sw.Elapsed.ToString("g"));

                sw.Restart();
                WriteTagsToModel(image, outputTags);

                sw.Stop();
                _logger.LogInformation("Post processing tags finished in {Time}", sw.Elapsed.ToString("g"));
                _logger.LogInformation("Finished Getting {Count} tags for {Image}", outputTags.Count,
                    image.GetPixivFilename());
            }
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unable to get tags for Images: {Exception}", exception);
        }
    }

    public List<(Image image, string[] Tags)> PredictMultipleImages(List<Image> images)
    {
        if (_transformer == null || _tags == null) return new List<(Image image, string[] Tags)>();
        var data = images.Select(image => new ModelInput { Data = GetImage(image.Blob) }).ToList();

        var result = _transformer.Transform(_mlContext.Data.LoadFromEnumerable(data));
        return images.Zip(_mlContext.Data.CreateEnumerable<ModelOutput>(result, false)).Select(a =>
                (a.First, _tags.Zip(a.Second.Scores).Where(b => b.Second > 0.85).Select(b => b.First).ToArray()))
            .ToList();
    }
        
    private static float[] GetImage(IEnumerable<byte> imageBlob)
    {
        using var mImage = new MagickImage(imageBlob.ToArray());
        mImage.Quality = 100;
        mImage.HasAlpha = true;
        mImage.Resize(new MagickGeometry($"{ModelInput.Width}>x{ModelInput.Height}>"));
        mImage.Extent(ModelInput.Width, ModelInput.Height, Gravity.Center, new MagickColor(255,255,255));
        mImage.HasAlpha = false;
        var pixels = mImage.GetPixels();
        var array = pixels.ToArray();
        if (array == null) return null;
        var data = new float[ModelInput.Width * ModelInput.Height * ModelInput.Channels];
        for (var index = 0; index < array.Length; index++)
        {
            data[index] = array[index] / 255.0f;
        }

        return data;
    }

    private void WriteTagsToModel(Image image, List<ImageTag> outputTags)
    {
        foreach (var tag in outputTags)
        {
            var edge = new ImageTagSource
            {
                Image = image,
                Tag = tag,
                Source = "DeepDanbooruModule"
            };

            if (image.TagSources.Any(a => a == edge)) continue;
            image.TagSources.Add(edge);
        }
    }
}