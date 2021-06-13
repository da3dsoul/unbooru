using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ImageInfrastructure.Core.Migrations
{
    public partial class AddImageBlob : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ImageBlobs",
                columns: table => new
                {
                    ImageBlobId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Data = table.Column<byte[]>(type: "BLOB", nullable: false),
                    ImageId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImageBlobs", x => x.ImageBlobId);
                    table.ForeignKey(
                        name: "FK_ImageBlobs_Images_ImageId",
                        column: x => x.ImageId,
                        principalTable: "Images",
                        principalColumn: "ImageId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ImageBlobs_ImageId",
                table: "ImageBlobs",
                column: "ImageId",
                unique: true);

            migrationBuilder.Sql("INSERT INTO ImageBlobs (ImageId, Data) SELECT ImageId, Blob FROM Images");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ImageBlobs");
        }
    }
}
