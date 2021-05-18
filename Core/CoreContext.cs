using System.IO;
using ImageInfrastructure.Abstractions.Interfaces;
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

        private ISettingsProvider<CoreSettings> SettingsProvider { get; set; }

        public CoreContext() {}
        
        public CoreContext(DbContextOptions<CoreContext> options, ISettingsProvider<CoreSettings> settingsProvider) : base(options)
        {
            SettingsProvider = settingsProvider;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string path;
            if (!string.IsNullOrEmpty(SettingsProvider?.Get(a => a.DatabasePath)))
            {
                Directory.CreateDirectory(SettingsProvider.Get(a => a.DatabasePath));
                path = Path.Combine(SettingsProvider.Get(a => a.DatabasePath), "Core.db3");
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