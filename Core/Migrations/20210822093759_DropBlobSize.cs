using Microsoft.EntityFrameworkCore.Migrations;

namespace ImageInfrastructure.Core.Migrations
{
    public partial class DropBlobSize : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ImageBlobs_Size",
                table: "ImageBlobs");

            migrationBuilder.DropColumn(
                name: "Size",
                table: "ImageBlobs");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "Size",
                table: "ImageBlobs",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_ImageBlobs_Size",
                table: "ImageBlobs",
                column: "Size");

            migrationBuilder.Sql("UPDATE ImageBlobs SET Size = (SELECT Image.Size FROM Images WHERE ImageBlobs.ImageId = Images.ImageId)");
        }
    }
}
