using Microsoft.EntityFrameworkCore.Migrations;

namespace ImageInfrastructure.Core.Migrations
{
    public partial class HopefullyFixArtistAccountMapping : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ArtistAccounts_Images_ImageId",
                table: "ArtistAccounts");

            migrationBuilder.DropIndex(
                name: "IX_ArtistAccounts_ImageId",
                table: "ArtistAccounts");

            migrationBuilder.DropColumn(
                name: "ImageId",
                table: "ArtistAccounts");

            migrationBuilder.CreateTable(
                name: "ArtistAccountImage",
                columns: table => new
                {
                    ArtistAccountsArtistAccountId = table.Column<int>(type: "INTEGER", nullable: false),
                    ImagesImageId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArtistAccountImage", x => new { x.ArtistAccountsArtistAccountId, x.ImagesImageId });
                    table.ForeignKey(
                        name: "FK_ArtistAccountImage_ArtistAccounts_ArtistAccountsArtistAccountId",
                        column: x => x.ArtistAccountsArtistAccountId,
                        principalTable: "ArtistAccounts",
                        principalColumn: "ArtistAccountId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ArtistAccountImage_Images_ImagesImageId",
                        column: x => x.ImagesImageId,
                        principalTable: "Images",
                        principalColumn: "ImageId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ArtistAccountImage_ImagesImageId",
                table: "ArtistAccountImage",
                column: "ImagesImageId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArtistAccountImage");

            migrationBuilder.AddColumn<int>(
                name: "ImageId",
                table: "ArtistAccounts",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ArtistAccounts_ImageId",
                table: "ArtistAccounts",
                column: "ImageId");

            migrationBuilder.AddForeignKey(
                name: "FK_ArtistAccounts_Images_ImageId",
                table: "ArtistAccounts",
                column: "ImageId",
                principalTable: "Images",
                principalColumn: "ImageId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
