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
using unbooru.Abstractions;
using unbooru.Abstractions.Attributes;
using unbooru.Abstractions.Enums;
using unbooru.Abstractions.Interfaces;
using unbooru.Abstractions.Poco;
using unbooru.Abstractions.Poco.Events;

namespace unbooru.DeepDanbooru
{
    public class DeepDanbooruModule : IModule
    {
        private readonly ILogger<DeepDanbooruModule> _logger;
        private readonly MLContext _mlContext;
        private readonly ITransformer _transformer;
        private readonly string[] _tags;
        
        public DeepDanbooruModule(ILogger<DeepDanbooruModule> logger)
        {
            _logger = logger;
            _mlContext = new MLContext(seed: 0);
            _transformer = GetBasicTransformer();
            
            var tagsLocation = Path.Combine(Arguments.DataPath, "DeepDanbooru", "tags.txt");
            if (!File.Exists(tagsLocation)) return;
            _tags = File.ReadAllLines(tagsLocation);
        }
        
        private ITransformer GetBasicTransformer()
        {
            var modelLocation = Path.Combine(Arguments.DataPath, "DeepDanbooru", "model-resnet-custom_v3.onnx");
            if (!File.Exists(modelLocation)) return null;
            // Define scoring pipeline
            var estimator = _mlContext.Transforms.ApplyOnnxModel(ModelOutput.OutputString, ModelInput.InputString,
                modelLocation);

            var transformer = estimator.Fit(_mlContext.Data.LoadFromEnumerable(Array.Empty<ModelInput>()));
            return transformer;
        }

        [ModulePostConfiguration(Priority = ModuleInitializationPriority.Metadata)]
        public void PostConfigure(IServiceProvider provider)
        {
            var imageProviders = provider.GetServices<IImageProvider>().ToList();
            if (imageProviders.Count == 0) return;
            foreach (var imageProvider in imageProviders)
            {
                imageProvider.ImageProvided += ImageProvided;
            }
        }
        
        [ModuleShutdown]
        public void Shutdown(IServiceProvider provider)
        {
            _logger.LogInformation("Shutting Down. Unregistering File Event Handlers");
            var imageProviders = provider.GetServices<IImageProvider>().ToList();
            if (imageProviders.Count == 0) return;
            foreach (var imageProvider in imageProviders)
            {
                imageProvider.ImageProvided -= ImageProvided;
            }
        }

        private void ImageProvided(object sender, ImageProvidedEventArgs e)
        {
            if (_transformer == null || _tags == null) return;
            FindTags(e).Wait();
        }

        private Task FindTags(ImageProvidedEventArgs e)
        {
            return FindTags(e.ServiceProvider, e.Images, e.CancellationToken);
        }

        public async Task FindTags(IServiceProvider provider, List<Image> images, CancellationToken token)
        {
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
        
        private List<(Image image, string[] Tags)> PredictMultipleImages(List<Image> images)
        {
            var data = images.Select(image => new ModelInput { Data = GetImage(image.Blob) }).ToList();

            var result = _transformer.Transform(_mlContext.Data.LoadFromEnumerable(data));
            return images.Zip(_mlContext.Data.CreateEnumerable<ModelOutput>(result, false)).Select(a =>
                    (a.First, _tags.Zip(a.Second.Scores).Where(b => b.Second > 0.85).Select(b => b.First).ToArray()))
                .ToList();
        }
        
        private static float[] GetImage(byte[] imageBlob)
        {
            using var mImage = new MagickImage(imageBlob);
            mImage.Quality = 100;
            mImage.HasAlpha = true;
            mImage.Resize(new MagickGeometry($"{ModelInput.Width}>x{ModelInput.Height}>"));
            mImage.Extent(ModelInput.Width, ModelInput.Height, Gravity.Center, new MagickColor(255,255,255));
            //mImage.BackgroundColor = new MagickColor(255, 255, 255);
            mImage.HasAlpha = false;
            var pixels = mImage.GetPixels();
            var array = pixels.ToArray();
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
                if (!image.Tags.Contains(tag))
                {
                    var edge = new ImageTagSource
                    {
                        Image = image,
                        Tag = tag,
                        Source = "DeepDanbooruModule"
                    };
                    image.TagSources.Add(edge);
                    //tag.TagSources.Add(edge);
                }
            }
        }

        public Task RunAsync(IServiceProvider provider, CancellationToken token)
        {
            return Task.CompletedTask;
        }
    }
}
