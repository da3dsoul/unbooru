using Microsoft.EntityFrameworkCore.Migrations;

namespace ImageInfrastructure.Core.Migrations
{
    public partial class AddPostUrlIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ImageSources_Uri_Source",
                table: "ImageSources");

            migrationBuilder.CreateIndex(
                name: "IX_ImageSources_Uri_PostUrl_Source",
                table: "ImageSources",
                columns: new[] { "Uri", "PostUrl", "Source" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ImageSources_Uri_PostUrl_Source",
                table: "ImageSources");

            migrationBuilder.CreateIndex(
                name: "IX_ImageSources_Uri_Source",
                table: "ImageSources",
                columns: new[] { "Uri", "Source" });
        }
    }
}
