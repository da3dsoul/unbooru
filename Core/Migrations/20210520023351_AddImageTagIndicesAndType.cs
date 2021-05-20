using Microsoft.EntityFrameworkCore.Migrations;

namespace ImageInfrastructure.Core.Migrations
{
    public partial class AddImageTagIndicesAndType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "ImageTags",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ImageTags_Name_Safety_Type",
                table: "ImageTags",
                columns: new[] { "Name", "Safety", "Type" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ImageTags_Name_Safety_Type",
                table: "ImageTags");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "ImageTags");
        }
    }
}
