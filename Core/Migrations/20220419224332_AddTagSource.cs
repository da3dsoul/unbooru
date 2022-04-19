using Microsoft.EntityFrameworkCore.Migrations;

namespace unbooru.Core.Migrations
{
    public partial class AddTagSource : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Source",
                table: "ImageTags",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Source",
                table: "ImageTags");
        }
    }
}
