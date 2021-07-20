﻿// <auto-generated />
using System;
using System.Collections.Generic;
using System.Reflection;
using ImageInfrastructure.Abstractions.Poco;
using Microsoft.EntityFrameworkCore.Metadata;

#pragma warning disable 219, 612, 618
#nullable disable

namespace ImageInfrastructure.Core.CompiledModels
{
    partial class RelatedImageEntityType
    {
        public static RuntimeEntityType Create(RuntimeModel model, RuntimeEntityType baseEntityType = null)
        {
            var runtimeEntityType = model.AddEntityType(
                "ImageInfrastructure.Abstractions.Poco.RelatedImage",
                typeof(RelatedImage),
                baseEntityType);

            var relatedImageId = runtimeEntityType.AddProperty(
                "RelatedImageId",
                typeof(int),
                propertyInfo: typeof(RelatedImage).GetProperty("RelatedImageId", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(RelatedImage).GetField("<RelatedImageId>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                valueGenerated: ValueGenerated.OnAdd,
                afterSaveBehavior: PropertySaveBehavior.Throw);

            var imageId = runtimeEntityType.AddProperty(
                "ImageId",
                typeof(int?),
                nullable: true);

            var imageSourceId = runtimeEntityType.AddProperty(
                "ImageSourceId",
                typeof(int?),
                nullable: true);

            var key = runtimeEntityType.AddKey(
                new[] { relatedImageId });
            runtimeEntityType.SetPrimaryKey(key);

            var index = runtimeEntityType.AddIndex(
                new[] { imageId });

            var index0 = runtimeEntityType.AddIndex(
                new[] { imageSourceId });

            return runtimeEntityType;
        }

        public static RuntimeForeignKey CreateForeignKey1(RuntimeEntityType declaringEntityType, RuntimeEntityType principalEntityType)
        {
            var runtimeForeignKey = declaringEntityType.AddForeignKey(new[] { declaringEntityType.FindProperty("ImageId") },
                principalEntityType.FindKey(new[] { principalEntityType.FindProperty("ImageId") }),
                principalEntityType);

            var image = declaringEntityType.AddNavigation("Image",
                runtimeForeignKey,
                onDependent: true,
                typeof(Image),
                propertyInfo: typeof(RelatedImage).GetProperty("Image", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(RelatedImage).GetField("<Image>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));

            var relatedImages = principalEntityType.AddNavigation("RelatedImages",
                runtimeForeignKey,
                onDependent: false,
                typeof(List<RelatedImage>),
                propertyInfo: typeof(Image).GetProperty("RelatedImages", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(Image).GetField("<RelatedImages>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));

            return runtimeForeignKey;
        }

        public static RuntimeForeignKey CreateForeignKey2(RuntimeEntityType declaringEntityType, RuntimeEntityType principalEntityType)
        {
            var runtimeForeignKey = declaringEntityType.AddForeignKey(new[] { declaringEntityType.FindProperty("ImageSourceId") },
                principalEntityType.FindKey(new[] { principalEntityType.FindProperty("ImageSourceId") }),
                principalEntityType);

            var imageSource = declaringEntityType.AddNavigation("ImageSource",
                runtimeForeignKey,
                onDependent: true,
                typeof(ImageSource),
                propertyInfo: typeof(RelatedImage).GetProperty("ImageSource", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(RelatedImage).GetField("<ImageSource>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));

            var relatedImages = principalEntityType.AddNavigation("RelatedImages",
                runtimeForeignKey,
                onDependent: false,
                typeof(List<RelatedImage>),
                propertyInfo: typeof(ImageSource).GetProperty("RelatedImages", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ImageSource).GetField("<RelatedImages>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));

            return runtimeForeignKey;
        }

        public static void CreateAnnotations(RuntimeEntityType runtimeEntityType)
        {
            runtimeEntityType.AddAnnotation("Relational:FunctionName", null);
            runtimeEntityType.AddAnnotation("Relational:Schema", null);
            runtimeEntityType.AddAnnotation("Relational:SqlQuery", null);
            runtimeEntityType.AddAnnotation("Relational:TableName", "RelatedImages");
            runtimeEntityType.AddAnnotation("Relational:ViewName", null);
            runtimeEntityType.AddAnnotation("Relational:ViewSchema", null);

            Customize(runtimeEntityType);
        }

        static partial void Customize(RuntimeEntityType runtimeEntityType);
    }
}
