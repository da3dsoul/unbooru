using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using unbooru.Abstractions.Interfaces;
using unbooru.Abstractions.Poco;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace unbooru.Core
{
    public class CoreContext : DbContext, IContext<ImageTag>, IContext<ArtistAccount>, IReadWriteContext<Image>, IReadWriteContext<ResponseCache>, IDatabaseContext
    {
        private readonly Dictionary<string, ArtistAccount> _artistCache = new(StringComparer.InvariantCultureIgnoreCase);
        private readonly ILogger<CoreContext> _logger;
        private readonly ISettingsProvider<CoreSettings> _settingsProvider;

        private readonly Dictionary<string, ImageTag> _tagCache = new(StringComparer.InvariantCultureIgnoreCase);

        public CoreContext() {}

        public CoreContext(DbContextOptions<CoreContext> options, ISettingsProvider<CoreSettings> settingsProvider, ILogger<CoreContext> logger) : base(options)
        {
            _settingsProvider = settingsProvider;
            _logger = logger;
            SavedChanges += OnSavedChanges;
        }

        [UsedImplicitly] public DbSet<Image> Images { get; set; }
        [UsedImplicitly] public DbSet<ImageBlob> ImageBlobs { get; set; }
        [UsedImplicitly] public DbSet<ArtistAccount> ArtistAccounts { get; set; }
        [UsedImplicitly] public DbSet<ImageSource> ImageSources { get; set; }
        [UsedImplicitly] public DbSet<RelatedImage> RelatedImages { get; set; }
        [UsedImplicitly] public DbSet<ImageTagSource> ImageImageTags { get; set; }
        [UsedImplicitly] public DbSet<ImageTag> ImageTags { get; set; }
        [UsedImplicitly] public DbSet<ResponseCache> ResponseCaches { get; set; }

        public async Task<ArtistAccount> Get(ArtistAccount artist, bool includeDepth = false, CancellationToken token = default)
        {
            var query = new Func<string, Task<ArtistAccount>>(url =>
                ArtistAccounts.Include(a => a.Images).OrderBy(a => a.ArtistAccountId)
                    .FirstOrDefaultAsync(a => a.Url == url, token));

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

        public bool DisableLogging { get; set; }

        public async Task<ImageTag> Get(ImageTag tag, bool includeDepth = false, CancellationToken token = default)
        {
            var query = new Func<string, Task<ImageTag>>(name =>
                ImageTags.OrderBy(a => a.ImageTagId)
                    .FirstOrDefaultAsync(a => a.Name == name, token));

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
            // this mess is 2 left joins. It's necessary
            //var imageImageTags = Set<Dictionary<string, object>>("ImageImageTag");
            /*var tempTags = await (from imageTag in Set<ImageTag>()
                join imageImageTag in imageImageTags
                    on imageTag.ImageTagId equals EF.Property<int>(imageImageTag, "TagsImageTagId") into grouping
                from imageImageTag in grouping.DefaultIfEmpty()
                join image in Set<Image>()
                    on EF.Property<int>(imageImageTag, "ImagesImageId") equals image.ImageId into grouping2
                from image in grouping2.DefaultIfEmpty()
                where namesToLookup.Contains(imageTag.Name)
                orderby imageTag.ImageTagId
                select new { imageTag, image }).ToListAsync(token);
            var existingTags = tempTags.GroupBy(a => a.imageTag, a => a.image).Select(a =>
            {
                var tag = a.Key;
                tag.Images = a.ToList();
                return tag;
            }).ToList();
            AttachRange(existingTags);*/

            var existingTags = await Set<ImageTag>().Where(a => namesToLookup.Contains(a.Name)).ToListAsync(token);

            foreach (var item in existingTags) _tagCache.Add(item.Name, item);

            var existingNames = existingTags.Select(a => a.Name).Concat(cachedTags.Select(a => a.Name)).ToHashSet();
            var nonExistingTags = items.Where(a => !existingNames.Contains(a.Name)).ToList();
            foreach (var item in nonExistingTags.Where(item => !_tagCache.ContainsKey(item.Name))) _tagCache.Add(item.Name, item);

            var results = existingTags.Concat(cachedTags).Concat(nonExistingTags).ToList();

            sw.Stop();
            if (!DisableLogging) _logger.LogInformation("Getting tags took {Time}", sw.Elapsed.ToString("g"));
            return results;
        }

        public Task<List<ImageTag>> FindAll(ImageTag item, bool includeDepth = false, CancellationToken token = default)
        {
            throw new InvalidOperationException("ImageTags are unique per name");
        }

        public T1 Execute<T1>(Func<IDatabaseContext, T1> func)
        {
            return func.Invoke(this);
        }

        IQueryable<T> IDatabaseContext.Set<T>(params Expression<Func<T, object>>[] includes)
        {
            if (includes == null || includes.Length <= 0) return base.Set<T>();

            var baseQuery = base.Set<T>().AsSplitQuery();
            foreach (var include in includes)
            {
                baseQuery = baseQuery.Include(include);
            }

            return baseQuery;
        }

        IQueryable<T> IDatabaseContext.ReadOnlySet<T>(params Expression<Func<T, object>>[] includes)
        {
            if (includes == null || includes.Length <= 0) return base.Set<T>().AsNoTrackingWithIdentityResolution();

            var baseQuery = base.Set<T>().AsNoTrackingWithIdentityResolution().AsSplitQuery();
            foreach (var include in includes)
            {
                baseQuery = baseQuery.Include(include);
            }

            return baseQuery;
        }

        public void Save()
        {
            SaveChanges();
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
                sourceIds = await ImageSources.Where(a => uris.Any(b => b == a.PostUrl)).Select(a => a.ImageSourceId)
                    .ToListAsync(token);
            }
            else
            {
                sourceIds = await ImageSources.Where(a => uris.Any(b => b == a.Uri)).Select(a => a.ImageSourceId)
                    .ToListAsync(token);
            }

            if (includeDepth)
            {
                // to perform merge operations, we need everything...
                // splitting this up to simplify the queries
                existingImages = await Images.AsSplitQuery()
                    .Include(a => a.TagSources)
                    .Include(a => a.Sources)
                    .Include(a => a.ArtistAccounts).ThenInclude(a => a.Images)
                    .Include(a => a.RelatedImages)
                    .Include(a => a.Blobs)
                    .Where(a => a.Sources.Any(b => sourceIds.Contains(b.ImageSourceId))).ToListAsync(token);
            }
            else
            {
                existingImages = await Images.Where(a => a.Sources.Any(b => sourceIds.Contains(b.ImageSourceId)))
                    .ToListAsync(token);
            }

            sw.Stop();
            if (!DisableLogging) _logger.LogInformation("Getting images took {Time}", sw.Elapsed.ToString("g"));
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
            if (!DisableLogging) _logger.LogInformation("Getting {Type} took {Time}", typeof(T).Name, sw.Elapsed.ToString("g"));
            return existingTag;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connectionString = _settingsProvider?.Get(a => a.ConnectionString);
            if (_settingsProvider != null && string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentException("Need Connection String");
            }

            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString = "Server=localhost;Database=unbooru;";
            }

            optionsBuilder.UseSqlServer(connectionString, builder => builder.CommandTimeout(3600));
            optionsBuilder.EnableSensitiveDataLogging();
            optionsBuilder.ConfigureWarnings(builder =>
            {
                builder.Log((Microsoft.EntityFrameworkCore.Diagnostics.CoreEventId.ContextInitialized, LogLevel.None));
                builder.Log((Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.CommandExecuted, LogLevel.None));
            });
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Table Override
            modelBuilder.Entity<ImageTagSource>().ToTable("ImageImageTag");

            // nullability
            modelBuilder.Entity<Image>().Property(a => a.Width).IsRequired();
            modelBuilder.Entity<Image>().Property(a => a.Height).IsRequired();
            modelBuilder.Entity<ImageSource>().Property(a => a.Source).IsRequired();//.UseCollation("NOCASE");
            modelBuilder.Entity<ImageSource>().Property(a => a.Uri).IsRequired();
            modelBuilder.Entity<ImageSource>().Property(a => a.Title).IsUnicode();
            modelBuilder.Entity<ImageTag>().Property(a => a.Name).IsRequired();//.UseCollation("NOCASE");
            modelBuilder.Entity<ArtistAccount>().Property(a => a.Id).IsRequired();
            modelBuilder.Entity<ArtistAccount>().Property(a => a.Url).IsRequired();
            modelBuilder.Entity<ArtistAccount>().Property(a => a.Name).IsRequired().IsUnicode();//.UseCollation("NOCASE");
            modelBuilder.Entity<ImageBlob>().Property(a => a.Data).IsRequired();

            // indexes
            modelBuilder.Entity<Image>().HasIndex(a => a.Width);
            modelBuilder.Entity<Image>().HasIndex(a => a.Height);
            modelBuilder.Entity<Image>().HasIndex(a => a.ImportDate);
            modelBuilder.Entity<Image>().HasIndex(a => a.Size);
            modelBuilder.Entity<ImageTag>().HasIndex(a => a.Name).IsUnique();
            modelBuilder.Entity<ImageTag>().HasIndex(a => new {a.Safety, a.Type});
            modelBuilder.Entity<ImageSource>().HasIndex(a => a.Uri);
            modelBuilder.Entity<ImageSource>().HasIndex(a => a.PostUrl);
            modelBuilder.Entity<ImageSource>().HasIndex(a => a.PostId);
            modelBuilder.Entity<ImageSource>().HasIndex(a => a.PostDate);
            modelBuilder.Entity<ImageSource>().HasIndex(a => a.Source);
            modelBuilder.Entity<ArtistAccount>().HasIndex(a => a.Id).IsUnique();
            modelBuilder.Entity<ArtistAccount>().HasIndex(a => a.Url).IsUnique();
            modelBuilder.Entity<ResponseCache>().HasIndex(a => a.Uri).IsUnique();
            modelBuilder.Entity<ResponseCache>().HasIndex(a => new {a.LastUpdated, a.StatusCode});
            modelBuilder.Entity<ImageTagSource>().HasIndex(e => e.TagsImageTagId, "IX_ImageImageTag_TagsImageTagId");
            modelBuilder.Entity<ImageHistogramColor>().HasIndex(e => e.ColorKey);
            modelBuilder.Entity<ImageComposition>().HasIndex(e => e.IsMonochrome);
            modelBuilder.Entity<ImageComposition>().HasIndex(e => e.IsGrayscale);
            modelBuilder.Entity<ImageComposition>().HasIndex(e => e.IsBlackAndWhite);

            // keys
            modelBuilder.Entity<ArtistAccount>().HasKey(a => a.ArtistAccountId);
            modelBuilder.Entity<ImageTagSource>().HasKey(e => new { e.ImagesImageId, e.TagsImageTagId, e.Source });

            // mappings
            modelBuilder.Entity<ArtistAccount>().HasMany(a => a.Images).WithMany(a => a.ArtistAccounts);
            modelBuilder.Entity<ImageBlob>().HasOne(a => a.Image).WithMany(a => a.Blobs).IsRequired();
            modelBuilder.Entity<ImageTagSource>().HasOne(d => d.Image).WithMany(p => p.TagSources).HasForeignKey(d => d.ImagesImageId);
            modelBuilder.Entity<ImageTagSource>().HasOne(d => d.Tag).WithMany(p => p.TagSources).HasForeignKey(d => d.TagsImageTagId);
            modelBuilder.Entity<ImageComposition>().HasOne(d => d.Image).WithOne(p => p.Composition).HasForeignKey<ImageComposition>(a => a.ImageId);

            // Auto-include
            modelBuilder.Entity<ImageTagSource>().Navigation(a => a.Tag).AutoInclude();
        }
    }
}
