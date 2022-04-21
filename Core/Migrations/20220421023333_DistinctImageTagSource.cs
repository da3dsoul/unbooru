using Microsoft.EntityFrameworkCore.Migrations;

namespace unbooru.Core.Migrations
{
    public partial class DistinctImageTagSource : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ImageImageTag",
                table: "ImageImageTag");

            migrationBuilder.AlterColumn<string>(
                name: "Source",
                table: "ImageImageTag",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ImageImageTag",
                table: "ImageImageTag",
                columns: new[] { "ImagesImageId", "TagsImageTagId", "Source" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ImageImageTag",
                table: "ImageImageTag");

            migrationBuilder.AlterColumn<string>(
                name: "Source",
                table: "ImageImageTag",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ImageImageTag",
                table: "ImageImageTag",
                columns: new[] { "ImagesImageId", "TagsImageTagId" });
        }
    }
}
