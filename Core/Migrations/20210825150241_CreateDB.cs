using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace unbooru.Core.Migrations
{
    public partial class CreateDB : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ArtistAccounts",
                columns: table => new
                {
                    ArtistAccountId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArtistAccounts", x => x.ArtistAccountId);
                });

            migrationBuilder.CreateTable(
                name: "Images",
                columns: table => new
                {
                    ImageId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Width = table.Column<int>(type: "int", nullable: false),
                    Height = table.Column<int>(type: "int", nullable: false),
                    ImportDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Size = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Images", x => x.ImageId);
                });

            migrationBuilder.CreateTable(
                name: "ImageTags",
                columns: table => new
                {
                    ImageTagId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Safety = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImageTags", x => x.ImageTagId);
                });

            migrationBuilder.CreateTable(
                name: "ResponseCaches",
                columns: table => new
                {
                    ResponseCacheId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Uri = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Response = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StatusCode = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResponseCaches", x => x.ResponseCacheId);
                });

            migrationBuilder.CreateTable(
                name: "ArtistAccountImage",
                columns: table => new
                {
                    ArtistAccountsArtistAccountId = table.Column<int>(type: "int", nullable: false),
                    ImagesImageId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArtistAccountImage", x => new { x.ArtistAccountsArtistAccountId, x.ImagesImageId });
                    table.ForeignKey(
                        name: "FK_ArtistAccountImage_ArtistAccounts_ArtistAccountsArtistAccountId",
                        column: x => x.ArtistAccountsArtistAccountId,
                        principalTable: "ArtistAccounts",
                        principalColumn: "ArtistAccountId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ArtistAccountImage_Images_ImagesImageId",
                        column: x => x.ImagesImageId,
                        principalTable: "Images",
                        principalColumn: "ImageId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ImageBlobs",
                columns: table => new
                {
                    ImageBlobId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Data = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    ImageId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImageBlobs", x => x.ImageBlobId);
                    table.ForeignKey(
                        name: "FK_ImageBlobs_Images_ImageId",
                        column: x => x.ImageId,
                        principalTable: "Images",
                        principalColumn: "ImageId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ImageSources",
                columns: table => new
                {
                    ImageSourceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Source = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    OriginalFilename = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Uri = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PostUrl = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    PostId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PostDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ImageId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImageSources", x => x.ImageSourceId);
                    table.ForeignKey(
                        name: "FK_ImageSources_Images_ImageId",
                        column: x => x.ImageId,
                        principalTable: "Images",
                        principalColumn: "ImageId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ImageImageTag",
                columns: table => new
                {
                    ImagesImageId = table.Column<int>(type: "int", nullable: false),
                    TagsImageTagId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImageImageTag", x => new { x.ImagesImageId, x.TagsImageTagId });
                    table.ForeignKey(
                        name: "FK_ImageImageTag_Images_ImagesImageId",
                        column: x => x.ImagesImageId,
                        principalTable: "Images",
                        principalColumn: "ImageId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ImageImageTag_ImageTags_TagsImageTagId",
                        column: x => x.TagsImageTagId,
                        principalTable: "ImageTags",
                        principalColumn: "ImageTagId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RelatedImages",
                columns: table => new
                {
                    RelatedImageId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ImageId = table.Column<int>(type: "int", nullable: true),
                    ImageSourceId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RelatedImages", x => x.RelatedImageId);
                    table.ForeignKey(
                        name: "FK_RelatedImages_Images_ImageId",
                        column: x => x.ImageId,
                        principalTable: "Images",
                        principalColumn: "ImageId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RelatedImages_ImageSources_ImageSourceId",
                        column: x => x.ImageSourceId,
                        principalTable: "ImageSources",
                        principalColumn: "ImageSourceId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ArtistAccountImage_ImagesImageId",
                table: "ArtistAccountImage",
                column: "ImagesImageId");

            migrationBuilder.CreateIndex(
                name: "IX_ArtistAccounts_Id",
                table: "ArtistAccounts",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ArtistAccounts_Url",
                table: "ArtistAccounts",
                column: "Url",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ImageBlobs_ImageId",
                table: "ImageBlobs",
                column: "ImageId");

            migrationBuilder.CreateIndex(
                name: "IX_ImageImageTag_TagsImageTagId",
                table: "ImageImageTag",
                column: "TagsImageTagId");

            migrationBuilder.CreateIndex(
                name: "IX_Images_Height",
                table: "Images",
                column: "Height");

            migrationBuilder.CreateIndex(
                name: "IX_Images_ImportDate",
                table: "Images",
                column: "ImportDate");

            migrationBuilder.CreateIndex(
                name: "IX_Images_Size",
                table: "Images",
                column: "Size");

            migrationBuilder.CreateIndex(
                name: "IX_Images_Width",
                table: "Images",
                column: "Width");

            migrationBuilder.CreateIndex(
                name: "IX_ImageSources_ImageId",
                table: "ImageSources",
                column: "ImageId");

            migrationBuilder.CreateIndex(
                name: "IX_ImageSources_PostDate",
                table: "ImageSources",
                column: "PostDate");

            migrationBuilder.CreateIndex(
                name: "IX_ImageSources_PostId",
                table: "ImageSources",
                column: "PostId");

            migrationBuilder.CreateIndex(
                name: "IX_ImageSources_PostUrl",
                table: "ImageSources",
                column: "PostUrl");

            migrationBuilder.CreateIndex(
                name: "IX_ImageSources_Source",
                table: "ImageSources",
                column: "Source");

            migrationBuilder.CreateIndex(
                name: "IX_ImageSources_Uri",
                table: "ImageSources",
                column: "Uri");

            migrationBuilder.CreateIndex(
                name: "IX_ImageTags_Name",
                table: "ImageTags",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ImageTags_Safety_Type",
                table: "ImageTags",
                columns: new[] { "Safety", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_RelatedImages_ImageId",
                table: "RelatedImages",
                column: "ImageId");

            migrationBuilder.CreateIndex(
                name: "IX_RelatedImages_ImageSourceId",
                table: "RelatedImages",
                column: "ImageSourceId");

            migrationBuilder.CreateIndex(
                name: "IX_ResponseCaches_LastUpdated_StatusCode",
                table: "ResponseCaches",
                columns: new[] { "LastUpdated", "StatusCode" });

            migrationBuilder.CreateIndex(
                name: "IX_ResponseCaches_Uri",
                table: "ResponseCaches",
                column: "Uri",
                unique: true,
                filter: "[Uri] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArtistAccountImage");

            migrationBuilder.DropTable(
                name: "ImageBlobs");

            migrationBuilder.DropTable(
                name: "ImageImageTag");

            migrationBuilder.DropTable(
                name: "RelatedImages");

            migrationBuilder.DropTable(
                name: "ResponseCaches");

            migrationBuilder.DropTable(
                name: "ArtistAccounts");

            migrationBuilder.DropTable(
                name: "ImageTags");

            migrationBuilder.DropTable(
                name: "ImageSources");

            migrationBuilder.DropTable(
                name: "Images");
        }
    }
}
