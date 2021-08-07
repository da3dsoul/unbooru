using Microsoft.EntityFrameworkCore.Migrations;

namespace ImageInfrastructure.Core.Migrations
{
    public partial class TagName_NOCASE : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "ImageTags",
                type: "TEXT",
                nullable: false,
                collation: "NOCASE",
                oldClrType: typeof(string),
                oldType: "TEXT");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "ImageTags",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldCollation: "NOCASE");
        }
    }
}
