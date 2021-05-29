using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ImageInfrastructure.Abstractions.Interfaces;
using ImageInfrastructure.Abstractions.Poco;
using ImageInfrastructure.Abstractions.Poco.Events;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using MoreLinq;

namespace ImageInfrastructure.SourceMerger
{
    public class SourceMergerModule : IModule
    {
        private readonly ILogger<SourceMergerModule> _logger;
        private readonly IContext<Image> _imageContext;
        
        public SourceMergerModule(ILogger<SourceMergerModule> logger, IContext<Image> imageContext)
        {
            _logger = logger;
            _imageContext = imageContext;
        }

        // We are just using this to ignore the warning. We have no way to test this yet
        [UsedImplicitly]
        private async Task MergeImages(ImageProvidedEventArgs e)
        {
            var results = await Task.WhenAll(e.Images.Select(async image =>
            {
                var images = await _imageContext.FindAll(image);
                var toRemove = images.ToList();
                var result = MergeImages(images);
                toRemove.Remove(result);
                toRemove.ForEach(a => _imageContext.Remove(a));
                return result;
            }));
            e.Images = results.ToList();
            await _imageContext.SaveChangesAsync();
        }
        
        private static Image MergeImages(IReadOnlyCollection<Image> images)
        {
            var first = images.FirstOrDefault();
            if (first == null) return null;
            // if we only have one, return it
            if (images.Count() == 1) return first;

            // merge them. use the lowest ID as a rule, then we can always remove the newer one in cleanup.
            var sources = images.SelectMany(a => a.Sources).DistinctBy(a => a.Uri).ToList();
            var artists = images.SelectMany(a => a.ArtistAccounts).DistinctBy(a => a.Url).ToList();
            var tags = images.SelectMany(a => a.Tags).DistinctBy(a => a.Name).ToList();
            
            // set everything to the first
            sources.ForEach(a => a.Image = first);
            artists.ForEach(a =>
            {
                // remove all of the ones that we merged
                images.Skip(1).Where(b => a.Images.Contains(b)).ForEach(b => a.Images.Remove(b));
                // add the first if it doesn't exist
                if (!a.Images.Contains(first)) a.Images.Add(first);
            });
            tags.ForEach(a =>
            {
                // remove all of the ones that we merged
                images.Skip(1).Where(b => a.Images.Contains(b)).ForEach(b => a.Images.Remove(b));
                // add the first if it doesn't exist
                if (!a.Images.Contains(first)) a.Images.Add(first);
            });
            first.Sources = sources;
            first.ArtistAccounts = artists;
            first.Tags = tags;

            // remap related images. First build a list of all image and image source pairs
            var related = images.SelectMany(a => a.RelatedImages).Where(a => !sources.Contains(a.ImageSource))
                .DistinctBy(a => new {a.Image.ImageId, a.ImageSource.ImageSourceId}).ToList();
            related.ForEach(a => a.Image = first);
            first.Sources.ForEach(a => a.RelatedImages = related);
            first.RelatedImages = related;

            return first;
        }

        public Task RunAsync(IServiceProvider provider, CancellationToken token)
        {
            _logger.LogInformation("Running {ModuleType} module", GetType());
            return Task.CompletedTask;
        }
    }
}