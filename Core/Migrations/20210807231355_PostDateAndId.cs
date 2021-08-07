using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ImageInfrastructure.Core.Migrations
{
    public partial class PostDateAndId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PostDate",
                table: "ImageSources",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PostId",
                table: "ImageSources",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ImportDate",
                table: "Images",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PostDate",
                table: "ImageSources");

            migrationBuilder.DropColumn(
                name: "PostId",
                table: "ImageSources");

            migrationBuilder.DropColumn(
                name: "ImportDate",
                table: "Images");
        }
    }
}
