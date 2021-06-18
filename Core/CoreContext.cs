using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ImageInfrastructure.Abstractions;
using ImageInfrastructure.Abstractions.Interfaces;
using ImageInfrastructure.Abstractions.Poco;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ImageInfrastructure.Core
{
    public class CoreContext : DbContext, IContext<ImageTag>, IContext<ArtistAccount>, IReadWriteContext<Image>, IReadWriteContext<ResponseCache>
    {
        private readonly ILogger<CoreContext> _logger;
        [UsedImplicitly] public DbSet<Image> Images { get; set; }
        [UsedImplicitly] public DbSet<ImageBlob> ImageBlobs { get; set; }
        [UsedImplicitly] public DbSet<ArtistAccount> ArtistAccounts { get; set; }
        [UsedImplicitly] public DbSet<ImageSource> ImageSources { get; set; }
        [UsedImplicitly] public DbSet<RelatedImage> RelatedImages { get; set; }
        [UsedImplicitly] public DbSet<ImageTag> ImageTags { get; set; }
        [UsedImplicitly] public DbSet<ResponseCache> ResponseCaches { get; set; }

        private readonly Dictionary<string, ImageTag> _tagCache = new(StringComparer.InvariantCultureIgnoreCase);
        private readonly Dictionary<string, ArtistAccount> _artistCache = new(StringComparer.InvariantCultureIgnoreCase);

        public CoreContext() {}
        
        public CoreContext(DbContextOptions<CoreContext> options, ILogger<CoreContext> logger) : base(options)
        {
            _logger = logger;
            SavedChanges += OnSavedChanges;
        }

        private void OnSavedChanges(object sender, SavedChangesEventArgs e)
        {
            _tagCache.Clear();
            _artistCache.Clear();
        }

        private async Task<T> Get<T>(IDictionary<string, T> cache, T item, Func<T, string> selector, Func<string, Task<T>> query) where T : class
        {
            var sw = Stopwatch.StartNew();
            var key = selector.Invoke(item);
            if (cache.ContainsKey(key))
            {
                var temp = cache[key];
                if (temp != null) return temp;
            }

            var existingTag = await query.Invoke(key);
            cache.Add(key, existingTag ?? item);

            sw.Stop();
            _logger.LogInformation("Getting {Type} took {Time}", typeof(T).Name, sw.Elapsed.ToString("g"));
            return existingTag;
        }

        public async Task<ImageTag> Get(ImageTag tag, bool includeDepth = false, CancellationToken token = default)
        {
            var query = new Func<string, Task<ImageTag>>(name =>
                ImageTags.AsSplitQuery().OrderBy(a => a.ImageTagId).FirstOrDefaultAsync(a => a.Name == name, token));

            return await Get(_tagCache, tag, a => a.Name, query);
        }

        public async Task<List<ImageTag>> Get(IReadOnlyList<ImageTag> items, bool includeDepth = false, CancellationToken token = default)
        {
            var sw = Stopwatch.StartNew();
            List<ImageTag> cachedTags = new();
            var tagsToLookup = items.ToList();

            // scan cache
            foreach (var item in items)
            {
                if (!_tagCache.ContainsKey(item.Name)) continue;
                var temp = _tagCache[item.Name];
                if (cachedTags.Contains(temp))
                {
                    tagsToLookup.Remove(item);
                    continue;
                }

                cachedTags.Add(temp);
                tagsToLookup.Remove(item);
            }

            var namesToLookup = tagsToLookup.Select(a => a.Name).Distinct().ToList();
            var existingTags = await ImageTags.AsSingleQuery()
                .Include(a => a.Images).Where(a => namesToLookup.Contains(a.Name))
                .ToListAsync(token);

            foreach (var item in existingTags.Where(item => !_tagCache.ContainsKey(item.Name))) _tagCache.Add(item.Name, item);

            var existingNames = existingTags.Select(a => a.Name).Concat(cachedTags.Select(a => a.Name)).ToHashSet();
            var nonExistingTags = items.Where(a => !existingNames.Contains(a.Name)).ToList();
            foreach (var item in nonExistingTags.Where(item => !_tagCache.ContainsKey(item.Name))) _tagCache.Add(item.Name, item);

            var results = existingTags.Concat(cachedTags).Concat(nonExistingTags).ToList();

            sw.Stop();
            _logger.LogInformation("Getting tags took {Time}", sw.Elapsed.ToString("g"));
            return results;
        }

        public Task<List<ImageTag>> FindAll(ImageTag item, bool includeDepth = false, CancellationToken token = default)
        {
            throw new InvalidOperationException("ImageTags are unique per name");
        }

        public async Task<ArtistAccount> Get(ArtistAccount artist, bool includeDepth = false, CancellationToken token = default)
        {
            var query = new Func<string, Task<ArtistAccount>>(url =>
                ArtistAccounts.OrderBy(a => a.ArtistAccountId).FirstOrDefaultAsync(a => a.Url == url, token));

            return await Get(_artistCache, artist, a => a.Url, query);
        }

        public Task<List<ArtistAccount>> Get(IReadOnlyList<ArtistAccount> items, bool includeDepth = false, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public Task<List<ArtistAccount>> FindAll(ArtistAccount item, bool includeDepth = false, CancellationToken token = default)
        {
            throw new InvalidOperationException("ArtistAccounts are unique per Url");
        }

        public async Task<Image> Get(Image image, bool includeDepth = false, CancellationToken token = default)
        {
            return (await FindAll(image, includeDepth, token))?.FirstOrDefault();
        }

        public Task<List<Image>> Get(IReadOnlyList<Image> items, bool includeDepth = false, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public async Task<List<Image>> FindAll(Image image, bool includeDepth = false, CancellationToken token = default)
        {
            var sw = Stopwatch.StartNew();
            var uris = image.Sources.Select(a => a.Uri).Where(a => a != null).ToList();

            List<int> sourceIds;
            List<Image> existingImages;
            if (!uris.Any())
            {
                uris = image.Sources.Select(a => a.PostUrl).Distinct().ToList();
                // to perform merge operations, we need everything...
                // splitting this up to simplify the queries
                sourceIds = await ImageSources.AsSplitQuery().IgnoreAutoIncludes().Include(a => a.Image)
                    .Where(a => uris.Any(b => b == a.PostUrl)).Select(a => a.ImageSourceId).ToListAsync(token);

                if (includeDepth)
                {
                    existingImages = await Images.AsSplitQuery()
                        .Where(a => a.Sources.Any(b => sourceIds.Contains(b.ImageSourceId))).ToListAsync(token);
                }
                else
                {
                    existingImages = await Images.AsSplitQuery().IgnoreAutoIncludes().Include(a => a.Sources)
                        .Include(a => a.ArtistAccounts).Include(a => a.RelatedImages)
                        .Where(a => a.Sources.Any(b => sourceIds.Contains(b.ImageSourceId))).ToListAsync(token);
                }

                return existingImages.Any() ? existingImages : new List<Image>();
            }

            // to perform merge operations, we need everything...
            // splitting this up to simplify the queries
            sourceIds = await ImageSources.AsSplitQuery().IgnoreAutoIncludes().Include(a => a.Image)
                .Where(a => uris.Any(b => b == a.Uri)).Select(a => a.ImageSourceId).ToListAsync(token);

            if (includeDepth)
            {
                existingImages = await Images.AsSplitQuery()
                    .Where(a => a.Sources.Any(b => sourceIds.Contains(b.ImageSourceId))).ToListAsync(token);
            }
            else
            {
                existingImages = await Images.AsSplitQuery().IgnoreAutoIncludes().Include(a => a.Sources)
                    .Include(a => a.ArtistAccounts).Include(a => a.RelatedImages)
                    .Where(a => a.Sources.Any(b => sourceIds.Contains(b.ImageSourceId))).ToListAsync(token);
            }

            sw.Stop();
            _logger.LogInformation("Getting images took {Time}", sw.Elapsed.ToString("g"));
            return existingImages.Any() ? existingImages : new List<Image>();
        }

        public async Task<Image> Add(Image item)
        {
            await Images.AddAsync(item);
            await SaveChangesAsync();
            return item;
        }

        public void Remove(Image item)
        {
            Images.Remove(item);
        }

        public async Task<ResponseCache> Get(ResponseCache item, bool includeDepth = false, CancellationToken token = default)
        {
            return await ResponseCaches.FirstOrDefaultAsync(a => a.Uri == item.Uri, token);
        }

        public Task<List<ResponseCache>> Get(IReadOnlyList<ResponseCache> items, bool includeDepth = false, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public Task<List<ResponseCache>> FindAll(ResponseCache item, bool includeDepth = false, CancellationToken token = default)
        {
            throw new InvalidOperationException("ResponseCache are unique per Uri");
        }

        public async Task<ResponseCache> Add(ResponseCache item)
        {
            await ResponseCaches.AddAsync(item);
            await SaveChangesAsync();
            return item;
        }

        public void Remove(ResponseCache item)
        {
            ResponseCaches.Remove(item);
        }

        public void RollbackChanges()
        {
            foreach (var entry in ChangeTracker.Entries())
            {
                switch (entry.State)
                {
                    case EntityState.Modified:
                    case EntityState.Deleted:
                        entry.State = EntityState.Modified; //Revert changes made to deleted entity.
                        entry.State = EntityState.Unchanged;
                        break;
                    case EntityState.Added:
                        entry.State = EntityState.Detached;
                        break;
                }
            }
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string path = string.IsNullOrEmpty(Arguments.DataPath) ? null : Path.Combine(Arguments.DataPath, "Database");
            if (!string.IsNullOrEmpty(path))
            {
                Directory.CreateDirectory(path);
                path = Path.Combine(path, "Core.db3");
            }
            else
            {
                path = "Core.db3";
            }
            optionsBuilder.UseSqlite($"Data Source={path};");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // nullability
            modelBuilder.Entity<Image>().Property(a => a.Width).IsRequired();
            modelBuilder.Entity<Image>().Property(a => a.Height).IsRequired();
            modelBuilder.Entity<ImageSource>().Property(a => a.Source).IsRequired();
            modelBuilder.Entity<ImageSource>().Property(a => a.Uri).IsRequired();
            modelBuilder.Entity<ImageSource>().Property(a => a.Title).IsUnicode();
            modelBuilder.Entity<ImageTag>().Property(a => a.Name).IsRequired();
            modelBuilder.Entity<ArtistAccount>().Property(a => a.Id).IsRequired();
            modelBuilder.Entity<ArtistAccount>().Property(a => a.Url).IsRequired();
            modelBuilder.Entity<ArtistAccount>().Property(a => a.Name).IsRequired().IsUnicode();
            modelBuilder.Entity<ImageBlob>().Property(a => a.Data).IsRequired();

            // indexes
            modelBuilder.Entity<ImageTag>().HasIndex(a => a.Name).IsUnique();
            modelBuilder.Entity<ImageTag>().HasIndex(a => new {a.Safety, a.Type});
            modelBuilder.Entity<ImageSource>().HasIndex(a => new {a.Uri, a.PostUrl, a.Source});
            modelBuilder.Entity<ArtistAccount>().HasIndex(a => a.Id).IsUnique();
            modelBuilder.Entity<ArtistAccount>().HasIndex(a => a.Url).IsUnique();
            modelBuilder.Entity<ResponseCache>().HasIndex(a => a.Uri).IsUnique();
            modelBuilder.Entity<ResponseCache>().HasIndex(a => new {a.LastUpdated, a.StatusCode});

            // keys
            modelBuilder.Entity<ArtistAccount>().HasKey(a => a.ArtistAccountId);

            // mappings
            modelBuilder.Entity<ArtistAccount>().HasMany(a => a.Images).WithMany(a => a.ArtistAccounts);
            modelBuilder.Entity<ImageBlob>().HasOne(a => a.Image).WithMany(a => a.Blobs).IsRequired();
        }
    }
}