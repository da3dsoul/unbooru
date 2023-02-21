using Microsoft.EntityFrameworkCore.Migrations;

namespace unbooru.Core.Migrations
{
    public partial class AddImageCompositionIndexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ImageComposition_IsBlackAndWhite",
                table: "ImageComposition",
                column: "IsBlackAndWhite");

            migrationBuilder.CreateIndex(
                name: "IX_ImageComposition_IsGrayscale",
                table: "ImageComposition",
                column: "IsGrayscale");

            migrationBuilder.CreateIndex(
                name: "IX_ImageComposition_IsMonochrome",
                table: "ImageComposition",
                column: "IsMonochrome");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ImageComposition_IsBlackAndWhite",
                table: "ImageComposition");

            migrationBuilder.DropIndex(
                name: "IX_ImageComposition_IsGrayscale",
                table: "ImageComposition");

            migrationBuilder.DropIndex(
                name: "IX_ImageComposition_IsMonochrome",
                table: "ImageComposition");
        }
    }
}
