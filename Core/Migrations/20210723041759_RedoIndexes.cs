using Microsoft.EntityFrameworkCore.Migrations;

namespace ImageInfrastructure.Core.Migrations
{
    public partial class RedoIndexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ImageSources_Uri_PostUrl_Source",
                table: "ImageSources");

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
                name: "IX_Images_Height",
                table: "Images",
                column: "Height");

            migrationBuilder.CreateIndex(
                name: "IX_Images_Width",
                table: "Images",
                column: "Width");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ImageSources_PostUrl",
                table: "ImageSources");

            migrationBuilder.DropIndex(
                name: "IX_ImageSources_Source",
                table: "ImageSources");

            migrationBuilder.DropIndex(
                name: "IX_ImageSources_Uri",
                table: "ImageSources");

            migrationBuilder.DropIndex(
                name: "IX_Images_Height",
                table: "Images");

            migrationBuilder.DropIndex(
                name: "IX_Images_Width",
                table: "Images");

            migrationBuilder.CreateIndex(
                name: "IX_ImageSources_Uri_PostUrl_Source",
                table: "ImageSources",
                columns: new[] { "Uri", "PostUrl", "Source" });
        }
    }
}
