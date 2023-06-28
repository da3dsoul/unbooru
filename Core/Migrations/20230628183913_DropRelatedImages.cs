using Microsoft.EntityFrameworkCore.Migrations;

namespace unbooru.Core.Migrations
{
    public partial class DropRelatedImages : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RelatedImages_ImageSources_ImageSourceId",
                table: "RelatedImages");

            migrationBuilder.DropIndex(
                name: "IX_RelatedImages_ImageSourceId",
                table: "RelatedImages");

            migrationBuilder.DropColumn(
                name: "ImageSourceId",
                table: "RelatedImages");

            migrationBuilder.Sql("DELETE FROM RelatedImages WHERE RelatedImageId > 0");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ImageSourceId",
                table: "RelatedImages",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RelatedImages_ImageSourceId",
                table: "RelatedImages",
                column: "ImageSourceId");

            migrationBuilder.AddForeignKey(
                name: "FK_RelatedImages_ImageSources_ImageSourceId",
                table: "RelatedImages",
                column: "ImageSourceId",
                principalTable: "ImageSources",
                principalColumn: "ImageSourceId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
