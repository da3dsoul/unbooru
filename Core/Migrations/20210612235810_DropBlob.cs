using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ImageInfrastructure.Core.Migrations
{
    public partial class DropBlob : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ImageBlobs_ImageId",
                table: "ImageBlobs");

            migrationBuilder.DropColumn(
                name: "Blob",
                table: "Images");

            migrationBuilder.CreateIndex(
                name: "IX_ImageBlobs_ImageId",
                table: "ImageBlobs",
                column: "ImageId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ImageBlobs_ImageId",
                table: "ImageBlobs");

            migrationBuilder.AddColumn<byte[]>(
                name: "Blob",
                table: "Images",
                type: "BLOB",
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.CreateIndex(
                name: "IX_ImageBlobs_ImageId",
                table: "ImageBlobs",
                column: "ImageId",
                unique: true);
        }
    }
}
