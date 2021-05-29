using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ImageInfrastructure.Abstractions;
using ImageInfrastructure.Abstractions.Interfaces;
using ImageInfrastructure.Abstractions.Poco;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using MoreLinq;

namespace ImageInfrastructure.Core
{
    public class CoreContext : DbContext, IContext<ImageTag>, IContext<ArtistAccount>, IContext<Image>
    {
        [UsedImplicitly] public DbSet<Image> Images { get; set; }
        [UsedImplicitly] public DbSet<ArtistAccount> ArtistAccounts { get; set; }
        [UsedImplicitly] public DbSet<ImageSource> ImageSources { get; set; }
        [UsedImplicitly] public DbSet<RelatedImage> RelatedImages { get; set; }
        [UsedImplicitly] public DbSet<ImageTag> ImageTags { get; set; }

        private static readonly Dictionary<string, ImageTag> TagNames = new();
        private static readonly Dictionary<string, ArtistAccount> ArtistUrls = new();
        private static readonly Dictionary<string, Image> ImageUris = new();

        public CoreContext() {}
        
        public CoreContext(DbContextOptions<CoreContext> options) : base(options)
        {
        }

        public async Task<ImageTag> Get(ImageTag tag, bool includeDepth = false)
        {
            if (TagNames.ContainsKey(tag.Name))
                return TagNames[tag.Name];

            var existingTag = await ImageTags.AsSplitQuery().Include(a => a.Images).OrderBy(a => a.ImageTagId)
                .FirstOrDefaultAsync(a => a.Name == tag.Name); if (existingTag != null)
            {
                TagNames.Add(existingTag.Name, existingTag);
                return existingTag;
            }

            TagNames.Add(tag.Name, tag);
            return null;
        }

        public Task<List<ImageTag>> FindAll(ImageTag item, bool includeDepth = false)
        {
            throw new InvalidOperationException("ImageTags are unique per name");
        }

        public void Remove(ImageTag item)
        {
            ImageTags.Remove(item);

            if (TagNames.ContainsKey(item.Name)) TagNames.Remove(item.Name);
        }

        public async Task<ArtistAccount> Get(ArtistAccount artist, bool includeDepth = false)
        {
            if (ArtistUrls.ContainsKey(artist.Url))
                return ArtistUrls[artist.Url];

            var existingArtist = await ArtistAccounts.AsSplitQuery().Include(a => a.Images)
                .OrderBy(a => a.ArtistAccountId).FirstOrDefaultAsync(a => a.Url == artist.Url); 
            if (existingArtist != null)
            {
                ArtistUrls.Add(existingArtist.Url, existingArtist);
                return existingArtist;
            }

            ArtistUrls.Add(artist.Url, artist);
            return null;
        }

        public Task<List<ArtistAccount>> FindAll(ArtistAccount item, bool includeDepth = false)
        {
            throw new InvalidOperationException("ArtistAccounts are unique per Url");
        }

        public void Remove(ArtistAccount item)
        {
            ArtistAccounts.Remove(item);

            if (ArtistUrls.ContainsKey(item.Name)) ArtistUrls.Remove(item.Name);
        }

        public async Task<Image> Get(Image image, bool includeDepth = false)
        {
            return (await FindAll(image, includeDepth)).FirstOrDefault();
        }

        public async Task<List<Image>> FindAll(Image image, bool includeDepth = false)
        {
            var uris = image.Sources.Select(a => a.Uri).ToList();
            var results = uris.Where(a => ImageUris.ContainsKey(a)).Select(a => ImageUris[a]).OrderBy(a => a.ImageId).ToList();
            if (results.Any()) return results;

            // to perform merge operations, we need everything...
            // splitting this up to simplify the queries
            var sourceIds = await ImageSources.AsSplitQuery().Include(a => a.Image)
                .Where(a => uris.Any(b => b == a.Uri)).Select(a => a.ImageSourceId).ToListAsync();

            List<Image> existingImages;
            if (includeDepth)
            {
                existingImages = await Images.AsSplitQuery().Include(a => a.Sources).ThenInclude(a => a.RelatedImages)
                    .Include(a => a.ArtistAccounts).Include(a => a.RelatedImages)
                    .Where(a => a.Sources.Any(b => sourceIds.Contains(b.ImageSourceId))).ToListAsync();
            }
            else
            {
                existingImages = await Images.AsSplitQuery().Include(a => a.Sources)
                    .Include(a => a.ArtistAccounts).Include(a => a.RelatedImages)
                    .Where(a => a.Sources.Any(b => sourceIds.Contains(b.ImageSourceId))).ToListAsync();
            }

            if (existingImages.Any())
            {
                var existingImage = existingImages.FirstOrDefault();
                uris.ForEach(a => ImageUris.Add(a, existingImage));
                return existingImages;
            }

            uris.ForEach(a => ImageUris.Add(a, image));

            return null;
        }

        public void Remove(Image item)
        {
            Images.Remove(item);
            
            var keys = ImageUris.Where(a => a.Value == item).Select(a => a.Key).Distinct();
            keys.ForEach(a => ImageUris.Remove(a));
        }

        private void Flush()
        {
            var changes = ChangeTracker.Entries<ImageTag>().Select(a => a.Entity.Name).Distinct().ToList();
            foreach (var change in changes)
            {
                if (TagNames.ContainsKey(change)) TagNames.Remove(change);
            }

            changes = ChangeTracker.Entries<ArtistAccount>().Select(a => a.Entity.Url).Distinct().ToList();
            foreach (var change in changes)
            {
                if (ArtistUrls.ContainsKey(change)) ArtistUrls.Remove(change);
            }
        }

        public override int SaveChanges()
        {
            Flush();
            return base.SaveChanges();
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            Flush();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            Flush();
            return base.SaveChangesAsync(cancellationToken);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = new CancellationToken())
        {
            Flush();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
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
            modelBuilder.Entity<ImageSource>().HasIndex(a => new {a.Uri, a.Source});
            modelBuilder.Entity<ArtistAccount>().HasIndex(a => new {a.Id}).IsUnique();
            modelBuilder.Entity<ArtistAccount>().HasIndex(a => new {a.Url}).IsUnique();
            
            // keys
            modelBuilder.Entity<ArtistAccount>().HasKey(a => a.ArtistAccountId);

            // mappings
            modelBuilder.Entity<ArtistAccount>().HasMany(a => a.Images).WithMany(a => a.ArtistAccounts);
        }
    }
}