﻿// <auto-generated />
using System;
using System.Collections.Generic;
using System.Reflection;
using ImageInfrastructure.Abstractions.Poco;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

#pragma warning disable 219, 612, 618
#nullable disable

namespace ImageInfrastructure.Core.CompiledModels
{
    partial class ImageBlobEntityType
    {
        public static RuntimeEntityType Create(RuntimeModel model, RuntimeEntityType baseEntityType = null)
        {
            var runtimeEntityType = model.AddEntityType(
                "ImageInfrastructure.Abstractions.Poco.ImageBlob",
                typeof(ImageBlob),
                baseEntityType);

            var imageBlobId = runtimeEntityType.AddProperty(
                "ImageBlobId",
                typeof(int),
                propertyInfo: typeof(ImageBlob).GetProperty("ImageBlobId", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ImageBlob).GetField("<ImageBlobId>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                valueGenerated: ValueGenerated.OnAdd,
                afterSaveBehavior: PropertySaveBehavior.Throw);

            var data = runtimeEntityType.AddProperty(
                "Data",
                typeof(byte[]),
                propertyInfo: typeof(ImageBlob).GetProperty("Data", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ImageBlob).GetField("<Data>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));

            var imageId = runtimeEntityType.AddProperty(
                "ImageId",
                typeof(int));

            var key = runtimeEntityType.AddKey(
                new[] { imageBlobId });
            runtimeEntityType.SetPrimaryKey(key);

            var index = runtimeEntityType.AddIndex(
                new[] { imageId });

            return runtimeEntityType;
        }

        public static RuntimeForeignKey CreateForeignKey1(RuntimeEntityType declaringEntityType, RuntimeEntityType principalEntityType)
        {
            var runtimeForeignKey = declaringEntityType.AddForeignKey(new[] { declaringEntityType.FindProperty("ImageId") },
                principalEntityType.FindKey(new[] { principalEntityType.FindProperty("ImageId") }),
                principalEntityType,
                deleteBehavior: DeleteBehavior.Cascade,
                required: true);

            var image = declaringEntityType.AddNavigation("Image",
                runtimeForeignKey,
                onDependent: true,
                typeof(Image),
                propertyInfo: typeof(ImageBlob).GetProperty("Image", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ImageBlob).GetField("<Image>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));

            var blobs = principalEntityType.AddNavigation("Blobs",
                runtimeForeignKey,
                onDependent: false,
                typeof(List<ImageBlob>),
                propertyInfo: typeof(Image).GetProperty("Blobs", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(Image).GetField("<Blobs>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));

            return runtimeForeignKey;
        }

        public static void CreateAnnotations(RuntimeEntityType runtimeEntityType)
        {
            runtimeEntityType.AddAnnotation("Relational:FunctionName", null);
            runtimeEntityType.AddAnnotation("Relational:Schema", null);
            runtimeEntityType.AddAnnotation("Relational:SqlQuery", null);
            runtimeEntityType.AddAnnotation("Relational:TableName", "ImageBlobs");
            runtimeEntityType.AddAnnotation("Relational:ViewName", null);
            runtimeEntityType.AddAnnotation("Relational:ViewSchema", null);

            Customize(runtimeEntityType);
        }

        static partial void Customize(RuntimeEntityType runtimeEntityType);
    }
}
