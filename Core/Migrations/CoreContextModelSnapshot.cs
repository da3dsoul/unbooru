﻿// <auto-generated />
using System;
using ImageInfrastructure.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ImageInfrastructure.Core.Migrations
{
    [DbContext(typeof(CoreContext))]
    partial class CoreContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.0-preview.3.21201.2");

            modelBuilder.Entity("ArtistAccountImage", b =>
                {
                    b.Property<int>("ArtistAccountsArtistAccountId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ImagesImageId")
                        .HasColumnType("INTEGER");

                    b.HasKey("ArtistAccountsArtistAccountId", "ImagesImageId");

                    b.HasIndex("ImagesImageId");

                    b.ToTable("ArtistAccountImage");
                });

            modelBuilder.Entity("ImageImageTag", b =>
                {
                    b.Property<int>("ImagesImageId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("TagsImageTagId")
                        .HasColumnType("INTEGER");

                    b.HasKey("ImagesImageId", "TagsImageTagId");

                    b.HasIndex("TagsImageTagId");

                    b.ToTable("ImageImageTag");
                });

            modelBuilder.Entity("ImageInfrastructure.Abstractions.Poco.ArtistAccount", b =>
                {
                    b.Property<int>("ArtistAccountId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.Property<string>("Url")
                        .HasColumnType("TEXT");

                    b.HasKey("ArtistAccountId");

                    b.HasIndex("Id")
                        .IsUnique();

                    b.ToTable("ArtistAccounts");
                });

            modelBuilder.Entity("ImageInfrastructure.Abstractions.Poco.Image", b =>
                {
                    b.Property<int>("ImageId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<byte[]>("Blob")
                        .HasColumnType("BLOB");

                    b.Property<int>("Height")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Width")
                        .HasColumnType("INTEGER");

                    b.HasKey("ImageId");

                    b.ToTable("Images");
                });

            modelBuilder.Entity("ImageInfrastructure.Abstractions.Poco.ImageSource", b =>
                {
                    b.Property<int>("ImageSourceId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Description")
                        .HasColumnType("TEXT");

                    b.Property<int?>("ImageId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("OriginalFilename")
                        .HasColumnType("TEXT");

                    b.Property<string>("Source")
                        .HasColumnType("TEXT");

                    b.Property<string>("Title")
                        .HasColumnType("TEXT");

                    b.Property<string>("Uri")
                        .HasColumnType("TEXT");

                    b.HasKey("ImageSourceId");

                    b.HasIndex("ImageId");

                    b.HasIndex("Uri", "Source");

                    b.ToTable("ImageSources");
                });

            modelBuilder.Entity("ImageInfrastructure.Abstractions.Poco.ImageTag", b =>
                {
                    b.Property<int>("ImageTagId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Description")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.Property<int>("Safety")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Type")
                        .HasColumnType("TEXT");

                    b.HasKey("ImageTagId");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.HasIndex("Safety", "Type");

                    b.ToTable("ImageTags");
                });

            modelBuilder.Entity("ImageInfrastructure.Abstractions.Poco.RelatedImage", b =>
                {
                    b.Property<int>("RelatedImageId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int?>("ImageId")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("ImageSourceId")
                        .HasColumnType("INTEGER");

                    b.HasKey("RelatedImageId");

                    b.HasIndex("ImageId");

                    b.HasIndex("ImageSourceId");

                    b.ToTable("RelatedImages");
                });

            modelBuilder.Entity("ArtistAccountImage", b =>
                {
                    b.HasOne("ImageInfrastructure.Abstractions.Poco.ArtistAccount", null)
                        .WithMany()
                        .HasForeignKey("ArtistAccountsArtistAccountId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ImageInfrastructure.Abstractions.Poco.Image", null)
                        .WithMany()
                        .HasForeignKey("ImagesImageId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("ImageImageTag", b =>
                {
                    b.HasOne("ImageInfrastructure.Abstractions.Poco.Image", null)
                        .WithMany()
                        .HasForeignKey("ImagesImageId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ImageInfrastructure.Abstractions.Poco.ImageTag", null)
                        .WithMany()
                        .HasForeignKey("TagsImageTagId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("ImageInfrastructure.Abstractions.Poco.ImageSource", b =>
                {
                    b.HasOne("ImageInfrastructure.Abstractions.Poco.Image", "Image")
                        .WithMany("Sources")
                        .HasForeignKey("ImageId");

                    b.Navigation("Image");
                });

            modelBuilder.Entity("ImageInfrastructure.Abstractions.Poco.RelatedImage", b =>
                {
                    b.HasOne("ImageInfrastructure.Abstractions.Poco.Image", "Image")
                        .WithMany("RelatedImages")
                        .HasForeignKey("ImageId");

                    b.HasOne("ImageInfrastructure.Abstractions.Poco.ImageSource", "ImageSource")
                        .WithMany("RelatedImages")
                        .HasForeignKey("ImageSourceId");

                    b.Navigation("Image");

                    b.Navigation("ImageSource");
                });

            modelBuilder.Entity("ImageInfrastructure.Abstractions.Poco.Image", b =>
                {
                    b.Navigation("RelatedImages");

                    b.Navigation("Sources");
                });

            modelBuilder.Entity("ImageInfrastructure.Abstractions.Poco.ImageSource", b =>
                {
                    b.Navigation("RelatedImages");
                });
#pragma warning restore 612, 618
        }
    }
}
