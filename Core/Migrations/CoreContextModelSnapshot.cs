﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using unbooru.Core;

namespace unbooru.Core.Migrations
{
    [DbContext(typeof(CoreContext))]
    partial class CoreContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "6.0.0-preview.5.21301.9")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("ArtistAccountImage", b =>
                {
                    b.Property<int>("ArtistAccountsArtistAccountId")
                        .HasColumnType("int");

                    b.Property<int>("ImagesImageId")
                        .HasColumnType("int");

                    b.HasKey("ArtistAccountsArtistAccountId", "ImagesImageId");

                    b.HasIndex("ImagesImageId");

                    b.ToTable("ArtistAccountImage");
                });

            modelBuilder.Entity("unbooru.Abstractions.Poco.ArtistAccount", b =>
                {
                    b.Property<int>("ArtistAccountId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<byte[]>("Avatar")
                        .HasColumnType("varbinary(max)");

                    b.Property<string>("Id")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .IsUnicode(true)
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Url")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("ArtistAccountId");

                    b.HasIndex("Id")
                        .IsUnique();

                    b.HasIndex("Url")
                        .IsUnique();

                    b.ToTable("ArtistAccounts");
                });

            modelBuilder.Entity("unbooru.Abstractions.Poco.Image", b =>
                {
                    b.Property<int>("ImageId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("Height")
                        .HasColumnType("int");

                    b.Property<DateTime>("ImportDate")
                        .HasColumnType("datetime2");

                    b.Property<long>("Size")
                        .HasColumnType("bigint");

                    b.Property<int>("Width")
                        .HasColumnType("int");

                    b.HasKey("ImageId");

                    b.HasIndex("Height");

                    b.HasIndex("ImportDate");

                    b.HasIndex("Size");

                    b.HasIndex("Width");

                    b.ToTable("Images");
                });

            modelBuilder.Entity("unbooru.Abstractions.Poco.ImageBlob", b =>
                {
                    b.Property<int>("ImageBlobId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<byte[]>("Data")
                        .IsRequired()
                        .HasColumnType("varbinary(max)");

                    b.Property<int>("ImageId")
                        .HasColumnType("int");

                    b.HasKey("ImageBlobId");

                    b.HasIndex("ImageId");

                    b.ToTable("ImageBlobs");
                });

            modelBuilder.Entity("unbooru.Abstractions.Poco.ImageComposition", b =>
                {
                    b.Property<int>("ImageCompositionId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("ImageId")
                        .HasColumnType("int");

                    b.Property<bool>("IsBlackAndWhite")
                        .HasColumnType("bit");

                    b.Property<bool>("IsGrayscale")
                        .HasColumnType("bit");

                    b.Property<bool>("IsMonochrome")
                        .HasColumnType("bit");

                    b.HasKey("ImageCompositionId");

                    b.HasIndex("ImageId")
                        .IsUnique();

                    b.HasIndex("IsBlackAndWhite");

                    b.HasIndex("IsGrayscale");

                    b.HasIndex("IsMonochrome");

                    b.ToTable("ImageComposition");
                });

            modelBuilder.Entity("unbooru.Abstractions.Poco.ImageHistogramColor", b =>
                {
                    b.Property<long>("ImageHistogramColorId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<long>("ColorKey")
                        .HasColumnType("bigint");

                    b.Property<int?>("CompositionImageCompositionId")
                        .HasColumnType("int");

                    b.Property<int>("Value")
                        .HasColumnType("int");

                    b.HasKey("ImageHistogramColorId");

                    b.HasIndex("ColorKey");

                    b.HasIndex("CompositionImageCompositionId");

                    b.ToTable("ImageHistogramColor");
                });

            modelBuilder.Entity("unbooru.Abstractions.Poco.ImageSource", b =>
                {
                    b.Property<int>("ImageSourceId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("ImageId")
                        .HasColumnType("int");

                    b.Property<string>("OriginalFilename")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("PostDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("PostId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("PostUrl")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Source")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Title")
                        .IsUnicode(true)
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Uri")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("ImageSourceId");

                    b.HasIndex("ImageId");

                    b.HasIndex("PostDate");

                    b.HasIndex("PostId");

                    b.HasIndex("PostUrl");

                    b.HasIndex("Source");

                    b.HasIndex("Uri");

                    b.ToTable("ImageSources");
                });

            modelBuilder.Entity("unbooru.Abstractions.Poco.ImageTag", b =>
                {
                    b.Property<int>("ImageTagId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("Safety")
                        .HasColumnType("int");

                    b.Property<string>("Type")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("ImageTagId");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.HasIndex("Safety", "Type");

                    b.ToTable("ImageTags");
                });

            modelBuilder.Entity("unbooru.Abstractions.Poco.ImageTagSource", b =>
                {
                    b.Property<int>("ImagesImageId")
                        .HasColumnType("int");

                    b.Property<int>("TagsImageTagId")
                        .HasColumnType("int");

                    b.Property<string>("Source")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("ImagesImageId", "TagsImageTagId", "Source");

                    b.HasIndex(new[] { "TagsImageTagId" }, "IX_ImageImageTag_TagsImageTagId");

                    b.ToTable("ImageImageTag");
                });

            modelBuilder.Entity("unbooru.Abstractions.Poco.RelatedImage", b =>
                {
                    b.Property<int>("RelatedImageId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int?>("ImageId")
                        .HasColumnType("int");

                    b.Property<int?>("RelationImageId")
                        .HasColumnType("int");

                    b.HasKey("RelatedImageId");

                    b.HasIndex("ImageId");

                    b.HasIndex("RelationImageId");

                    b.ToTable("RelatedImages");
                });

            modelBuilder.Entity("unbooru.Abstractions.Poco.ResponseCache", b =>
                {
                    b.Property<int>("ResponseCacheId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("LastUpdated")
                        .HasColumnType("datetime2");

                    b.Property<string>("Response")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("StatusCode")
                        .HasColumnType("int");

                    b.Property<string>("Uri")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("ResponseCacheId");

                    b.HasIndex("Uri")
                        .IsUnique()
                        .HasFilter("[Uri] IS NOT NULL");

                    b.HasIndex("LastUpdated", "StatusCode");

                    b.ToTable("ResponseCaches");
                });

            modelBuilder.Entity("ArtistAccountImage", b =>
                {
                    b.HasOne("unbooru.Abstractions.Poco.ArtistAccount", null)
                        .WithMany()
                        .HasForeignKey("ArtistAccountsArtistAccountId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("unbooru.Abstractions.Poco.Image", null)
                        .WithMany()
                        .HasForeignKey("ImagesImageId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("unbooru.Abstractions.Poco.ImageBlob", b =>
                {
                    b.HasOne("unbooru.Abstractions.Poco.Image", "Image")
                        .WithMany("Blobs")
                        .HasForeignKey("ImageId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Image");
                });

            modelBuilder.Entity("unbooru.Abstractions.Poco.ImageComposition", b =>
                {
                    b.HasOne("unbooru.Abstractions.Poco.Image", "Image")
                        .WithOne("Composition")
                        .HasForeignKey("unbooru.Abstractions.Poco.ImageComposition", "ImageId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Image");
                });

            modelBuilder.Entity("unbooru.Abstractions.Poco.ImageHistogramColor", b =>
                {
                    b.HasOne("unbooru.Abstractions.Poco.ImageComposition", "Composition")
                        .WithMany("Histogram")
                        .HasForeignKey("CompositionImageCompositionId");

                    b.Navigation("Composition");
                });

            modelBuilder.Entity("unbooru.Abstractions.Poco.ImageSource", b =>
                {
                    b.HasOne("unbooru.Abstractions.Poco.Image", "Image")
                        .WithMany("Sources")
                        .HasForeignKey("ImageId");

                    b.Navigation("Image");
                });

            modelBuilder.Entity("unbooru.Abstractions.Poco.ImageTagSource", b =>
                {
                    b.HasOne("unbooru.Abstractions.Poco.Image", "Image")
                        .WithMany("TagSources")
                        .HasForeignKey("ImagesImageId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("unbooru.Abstractions.Poco.ImageTag", "Tag")
                        .WithMany("TagSources")
                        .HasForeignKey("TagsImageTagId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Image");

                    b.Navigation("Tag");
                });

            modelBuilder.Entity("unbooru.Abstractions.Poco.RelatedImage", b =>
                {
                    b.HasOne("unbooru.Abstractions.Poco.Image", "Image")
                        .WithMany("RelatedImages")
                        .HasForeignKey("ImageId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("unbooru.Abstractions.Poco.Image", "Relation")
                        .WithMany()
                        .HasForeignKey("RelationImageId");

                    b.Navigation("Image");

                    b.Navigation("Relation");
                });

            modelBuilder.Entity("unbooru.Abstractions.Poco.Image", b =>
                {
                    b.Navigation("Blobs");

                    b.Navigation("Composition");

                    b.Navigation("RelatedImages");

                    b.Navigation("Sources");

                    b.Navigation("TagSources");
                });

            modelBuilder.Entity("unbooru.Abstractions.Poco.ImageComposition", b =>
                {
                    b.Navigation("Histogram");
                });

            modelBuilder.Entity("unbooru.Abstractions.Poco.ImageTag", b =>
                {
                    b.Navigation("TagSources");
                });
#pragma warning restore 612, 618
        }
    }
}
