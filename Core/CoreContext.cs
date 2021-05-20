using System.IO;
using ImageInfrastructure.Abstractions;
using ImageInfrastructure.Core.Models;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace ImageInfrastructure.Core
{
    public class CoreContext : DbContext
    {
        [UsedImplicitly] public DbSet<Image> Images { get; set; }
        [UsedImplicitly] public DbSet<ArtistAccount> ArtistAccounts { get; set; }
        [UsedImplicitly] public DbSet<ImageSource> ImageSources { get; set; }
        [UsedImplicitly] public DbSet<ImageTag> ImageTags { get; set; }

        public CoreContext() {}
        
        public CoreContext(DbContextOptions<CoreContext> options) : base(options)
        {
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