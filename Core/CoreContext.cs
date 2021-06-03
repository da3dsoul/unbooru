using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ImageInfrastructure.Abstractions;
using ImageInfrastructure.Abstractions.Interfaces;
using ImageInfrastructure.Abstractions.Poco;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace ImageInfrastructure.Core
{
    public class CoreContext : DbContext, IContext<ImageTag>, IContext<ArtistAccount>, IReadWriteContext<Image>, IReadWriteContext<ResponseCache>
    {
        [UsedImplicitly] public DbSet<Image> Images { get; set; }
        [UsedImplicitly] public DbSet<ArtistAccount> ArtistAccounts { get; set; }
        [UsedImplicitly] public DbSet<ImageSource> ImageSources { get; set; }
        [UsedImplicitly] public DbSet<RelatedImage> RelatedImages { get; set; }
        [UsedImplicitly] public DbSet<ImageTag> ImageTags { get; set; }
        [UsedImplicitly] public DbSet<ResponseCache> ResponseCaches { get; set; }

        private readonly Dictionary<string, ImageTag> _tagCache = new(StringComparer.InvariantCultureIgnoreCase);
        private readonly Dictionary<string, ArtistAccount> _artistCache = new(StringComparer.InvariantCultureIgnoreCase);

        public CoreContext() {}
        
        public CoreContext(DbContextOptions<CoreContext> options) : base(options)
        {
            SavedChanges += OnSavedChanges;
        }

        private void OnSavedChanges(object sender, SavedChangesEventArgs e)
        {
            _tagCache.Clear();
        }

        private static async Task<T> Get<T>(IDictionary<string, T> cache, T item, Func<T, string> selector, Func<string, Task<T>> query) where T : class
        {
            var key = selector.Invoke(item);
            if (cache.ContainsKey(key))
            {
                var temp = cache[key];
                if (temp != null) return temp;
            }

            var existingTag = await query.Invoke(key);
            cache.Add(key, existingTag ?? item);

            return existingTag;
        }

        public async Task<ImageTag> Get(ImageTag tag, bool includeDepth = false)
        {
            var query = new Func<string, Task<ImageTag>>(name =>
                ImageTags.AsSplitQuery().OrderBy(a => a.ImageTagId).FirstOrDefaultAsync(a => a.Name == name));

            return await Get(_tagCache, tag, a => a.Name, query);
        }

        public Task<List<ImageTag>> FindAll(ImageTag item, bool includeDepth = false)
        {
            throw new InvalidOperationException("ImageTags are unique per name");
        }

        public async Task<ArtistAccount> Get(ArtistAccount artist, bool includeDepth = false)
        {
            var query = new Func<string, Task<ArtistAccount>>(url =>
                ArtistAccounts.OrderBy(a => a.ArtistAccountId).FirstOrDefaultAsync(a => a.Url == url));

            return await Get(_artistCache, artist, a => a.Url, query);
        }

        public Task<List<ArtistAccount>> FindAll(ArtistAccount item, bool includeDepth = false)
        {
            throw new InvalidOperationException("ArtistAccounts are unique per Url");
        }

        public async Task<Image> Get(Image image, bool includeDepth = false)
        {
            return (await FindAll(image, includeDepth))?.FirstOrDefault();
        }

        public async Task<List<Image>> FindAll(Image image, bool includeDepth = false)
        {
            var uris = image.Sources.Select(a => a.Uri).Where(a => a != null).ToList();

            List<int> sourceIds;
            List<Image> existingImages;
            if (!uris.Any())
            {
                uris = image.Sources.Select(a => a.PostUrl).Distinct().ToList();
                // to perform merge operations, we need everything...
                // splitting this up to simplify the queries
                sourceIds = await ImageSources.AsSplitQuery().IgnoreAutoIncludes().Include(a => a.Image)
                    .Where(a => uris.Any(b => b == a.PostUrl)).Select(a => a.ImageSourceId).ToListAsync();

                if (includeDepth)
                {
                    existingImages = await Images.AsSplitQuery()
                        .Where(a => a.Sources.Any(b => sourceIds.Contains(b.ImageSourceId))).ToListAsync();
                }
                else
                {
                    existingImages = await Images.AsSplitQuery().IgnoreAutoIncludes().Include(a => a.Sources)
                        .Include(a => a.ArtistAccounts).Include(a => a.RelatedImages)
                        .Where(a => a.Sources.Any(b => sourceIds.Contains(b.ImageSourceId))).ToListAsync();
                }

                return existingImages.Any() ? existingImages : new List<Image>();
            }

            // to perform merge operations, we need everything...
            // splitting this up to simplify the queries
            sourceIds = await ImageSources.AsSplitQuery().IgnoreAutoIncludes().Include(a => a.Image)
                .Where(a => uris.Any(b => b == a.Uri)).Select(a => a.ImageSourceId).ToListAsync();

            if (includeDepth)
            {
                existingImages = await Images.AsSplitQuery()
                    .Where(a => a.Sources.Any(b => sourceIds.Contains(b.ImageSourceId))).ToListAsync();
            }
            else
            {
                existingImages = await Images.AsSplitQuery().IgnoreAutoIncludes().Include(a => a.Sources)
                    .Include(a => a.ArtistAccounts).Include(a => a.RelatedImages)
                    .Where(a => a.Sources.Any(b => sourceIds.Contains(b.ImageSourceId))).ToListAsync();
            }

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

        public async Task<ResponseCache> Get(ResponseCache item, bool includeDepth = false)
        {
            return await ResponseCaches.FirstOrDefaultAsync(a => a.Uri == item.Uri);
        }

        public Task<List<ResponseCache>> FindAll(ResponseCache item, bool includeDepth = false)
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
            modelBuilder.Entity<Image>().Property(a => a.Blob).IsRequired();
            modelBuilder.Entity<Image>().Property(a => a.Width).IsRequired();
            modelBuilder.Entity<Image>().Property(a => a.Height).IsRequired();
            modelBuilder.Entity<ImageSource>().Property(a => a.Source).IsRequired();
            modelBuilder.Entity<ImageSource>().Property(a => a.Uri).IsRequired();
            modelBuilder.Entity<ImageSource>().Property(a => a.Title).IsUnicode();
            modelBuilder.Entity<ImageTag>().Property(a => a.Name).IsRequired();
            modelBuilder.Entity<ArtistAccount>().Property(a => a.Id).IsRequired();
            modelBuilder.Entity<ArtistAccount>().Property(a => a.Url).IsRequired();
            modelBuilder.Entity<ArtistAccount>().Property(a => a.Name).IsRequired().IsUnicode();

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
        }
    }
}