using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ImageInfrastructure.Abstractions;
using ImageInfrastructure.Abstractions.Interfaces.Contexts;
using ImageInfrastructure.Abstractions.Poco;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace ImageInfrastructure.Core
{
    public class CoreContext : DbContext, ITagContext, IArtistContext
    {
        [UsedImplicitly] public DbSet<Image> Images { get; set; }
        [UsedImplicitly] public DbSet<ArtistAccount> ArtistAccounts { get; set; }
        [UsedImplicitly] public DbSet<ImageSource> ImageSources { get; set; }
        [UsedImplicitly] public DbSet<RelatedImage> RelatedImages { get; set; }
        [UsedImplicitly] public DbSet<ImageTag> ImageTags { get; set; }

        private static readonly Dictionary<string, ImageTag> TagNames = new();
        private static readonly Dictionary<string, ArtistAccount> ArtistIds = new();

        public CoreContext() {}
        
        public CoreContext(DbContextOptions<CoreContext> options) : base(options)
        {
        }

        public async Task<ImageTag> GetTag(ImageTag tag)
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

        public void FlushTags()
        {
            var changes = ChangeTracker.Entries<ImageTag>().Select(a => a.Entity.Name).Distinct().ToList();
            foreach (var change in changes)
            {
                if (TagNames.ContainsKey(change)) TagNames.Remove(change);
            }
        }
        
        public async Task<ArtistAccount> GetArtist(ArtistAccount artist)
        {
            if (ArtistIds.ContainsKey(artist.Id))
                return ArtistIds[artist.Id];

            var existingArtist = await ArtistAccounts.AsSplitQuery().Include(a => a.Images)
                .OrderBy(a => a.ArtistAccountId).FirstOrDefaultAsync(a => a.Id == artist.Id); 
            if (existingArtist != null)
            {
                ArtistIds.Add(existingArtist.Id, existingArtist);
                return existingArtist;
            }

            ArtistIds.Add(artist.Id, artist);
            return null;
        }

        public void FlushArtists()
        {
            var changes = ChangeTracker.Entries<ArtistAccount>().Select(a => a.Entity.Id).Distinct().ToList();
            foreach (var change in changes)
            {
                if (ArtistIds.ContainsKey(change)) ArtistIds.Remove(change);
            }
        }

        public override int SaveChanges()
        {
            FlushTags();
            FlushArtists();
            return base.SaveChanges();
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            FlushTags();
            FlushArtists();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            FlushTags();
            FlushArtists();
            return base.SaveChangesAsync(cancellationToken);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = new CancellationToken())
        {
            FlushTags();
            FlushArtists();
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
            
            // keys
            modelBuilder.Entity<ArtistAccount>().HasKey(a => a.ArtistAccountId);

            // mappings
            modelBuilder.Entity<ArtistAccount>().HasMany(a => a.Images).WithMany(a => a.ArtistAccounts);
        }
    }
}