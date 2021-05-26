using Microsoft.EntityFrameworkCore.Migrations;

namespace ImageInfrastructure.Core.Migrations
{
    public partial class ReaddIndexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Id",
                table: "ArtistAccounts",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ImageTags_Name",
                table: "ImageTags",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ImageTags_Safety_Type",
                table: "ImageTags",
                columns: new[] { "Safety", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_ImageSources_Uri_Source",
                table: "ImageSources",
                columns: new[] { "Uri", "Source" });

            migrationBuilder.CreateIndex(
                name: "IX_ArtistAccounts_Id",
                table: "ArtistAccounts",
                column: "Id",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ImageTags_Name",
                table: "ImageTags");

            migrationBuilder.DropIndex(
                name: "IX_ImageTags_Safety_Type",
                table: "ImageTags");

            migrationBuilder.DropIndex(
                name: "IX_ImageSources_Uri_Source",
                table: "ImageSources");

            migrationBuilder.DropIndex(
                name: "IX_ArtistAccounts_Id",
                table: "ArtistAccounts");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "ArtistAccounts");
        }
    }
}
