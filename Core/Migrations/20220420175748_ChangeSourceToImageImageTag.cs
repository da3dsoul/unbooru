using Microsoft.EntityFrameworkCore.Migrations;

namespace unbooru.Core.Migrations
{
    public partial class ChangeSourceToImageImageTag : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Source",
                table: "ImageTags");

            migrationBuilder.AddColumn<string>(
                name: "Source",
                table: "ImageImageTag",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Source",
                table: "ImageImageTag");

            migrationBuilder.AddColumn<string>(
                name: "Source",
                table: "ImageTags",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
