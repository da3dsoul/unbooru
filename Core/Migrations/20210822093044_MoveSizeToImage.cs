using Microsoft.EntityFrameworkCore.Migrations;

namespace ImageInfrastructure.Core.Migrations
{
    public partial class MoveSizeToImage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "Size",
                table: "Images",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_Images_Size",
                table: "Images",
                column: "Size");

            migrationBuilder.Sql("UPDATE Images SET Size = (SELECT ImageBlobs.Size FROM ImageBlobs WHERE ImageBlobs.ImageId = Images.ImageId)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Images_Size",
                table: "Images");

            migrationBuilder.DropColumn(
                name: "Size",
                table: "Images");
        }
    }
}
