using Microsoft.EntityFrameworkCore.Migrations;

namespace ImageInfrastructure.Core.Migrations
{
    public partial class AddDateAndSizeIndices : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ImageSources_PostUrl",
                table: "ImageSources");

            migrationBuilder.AlterColumn<string>(
                name: "Source",
                table: "ImageSources",
                type: "TEXT",
                nullable: false,
                collation: "NOCASE",
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AddColumn<long>(
                name: "Size",
                table: "ImageBlobs",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "ArtistAccounts",
                type: "TEXT",
                nullable: false,
                collation: "NOCASE",
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.CreateIndex(
                name: "IX_ImageSources_PostDate",
                table: "ImageSources",
                column: "PostDate");

            migrationBuilder.CreateIndex(
                name: "IX_ImageSources_PostId",
                table: "ImageSources",
                column: "PostId");

            migrationBuilder.CreateIndex(
                name: "IX_ImageSources_PostUrl",
                table: "ImageSources",
                column: "PostUrl");

            migrationBuilder.CreateIndex(
                name: "IX_Images_ImportDate",
                table: "Images",
                column: "ImportDate");

            migrationBuilder.CreateIndex(
                name: "IX_ImageBlobs_Size",
                table: "ImageBlobs",
                column: "Size");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ImageSources_PostDate",
                table: "ImageSources");

            migrationBuilder.DropIndex(
                name: "IX_ImageSources_PostId",
                table: "ImageSources");

            migrationBuilder.DropIndex(
                name: "IX_ImageSources_PostUrl",
                table: "ImageSources");

            migrationBuilder.DropIndex(
                name: "IX_Images_ImportDate",
                table: "Images");

            migrationBuilder.DropIndex(
                name: "IX_ImageBlobs_Size",
                table: "ImageBlobs");

            migrationBuilder.DropColumn(
                name: "Size",
                table: "ImageBlobs");

            migrationBuilder.AlterColumn<string>(
                name: "Source",
                table: "ImageSources",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldCollation: "NOCASE");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "ArtistAccounts",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldCollation: "NOCASE");

            migrationBuilder.CreateIndex(
                name: "IX_ImageSources_PostUrl",
                table: "ImageSources",
                column: "PostUrl");
        }
    }
}
