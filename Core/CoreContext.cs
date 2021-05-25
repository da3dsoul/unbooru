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

namespace ImageInfrastructure.Core
{
    public class CoreContext : DbContext, ITagContext
    {
        [UsedImplicitly] public DbSet<Image> Images { get; set; }
        [UsedImplicitly] public DbSet<ArtistAccount> ArtistAccounts { get; set; }
        [UsedImplicitly] public DbSet<ImageSource> ImageSources { get; set; }
        [UsedImplicitly] public DbSet<ImageTag> ImageTags { get; set; }

        private readonly Dictionary<string, ImageTag> _tagNames = new();

        public CoreContext() {}
        
        public CoreContext(DbContextOptions<CoreContext> options) : base(options)
        {
        }

        public bool GetTag(ImageTag tag, out ImageTag existing)
        {
            if (_tagNames.ContainsKey(tag.Name))
            {
                existing = tag;
                return true;
            }

            var existingTag = ImageTags.FirstOrDefault(a => a.Name == tag.Name);
            if (existingTag != null)
            {
                _tagNames.Add(existingTag.Name, existingTag);
                existing = existingTag;
                return true;
            }

            _tagNames.Add(tag.Name, tag);
            existing = null;
            return false;
        }

        public void FlushTags()
        {
            var changes = ChangeTracker.Entries<ImageTag>().Select(a => a.Entity.Name).ToList();
            foreach (var change in changes)
            {
                if (_tagNames.ContainsKey(change)) _tagNames.Remove(change);
            }
        }

        public override int SaveChanges()
        {
            FlushTags();
            return base.SaveChanges();
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            FlushTags();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            FlushTags();
            return base.SaveChangesAsync(cancellationToken);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = new CancellationToken())
        {
            FlushTags();
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
            optionsBuilder.UseSqlite(
                $"Data Source={path};");
        }
    }
}