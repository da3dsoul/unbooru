using Microsoft.EntityFrameworkCore.Migrations;

namespace unbooru.Core.Migrations
{
    public partial class ReaddRelatedImages : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RelationImageId",
                table: "RelatedImages",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RelatedImages_RelationImageId",
                table: "RelatedImages",
                column: "RelationImageId");

            migrationBuilder.AddForeignKey(
                name: "FK_RelatedImages_Images_RelationImageId",
                table: "RelatedImages",
                column: "RelationImageId",
                principalTable: "Images",
                principalColumn: "ImageId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RelatedImages_Images_RelationImageId",
                table: "RelatedImages");

            migrationBuilder.DropIndex(
                name: "IX_RelatedImages_RelationImageId",
                table: "RelatedImages");

            migrationBuilder.DropColumn(
                name: "RelationImageId",
                table: "RelatedImages");
        }
    }
}
