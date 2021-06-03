using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ImageInfrastructure.Core.Migrations
{
    public partial class AddResponseCache : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ResponseCaches",
                columns: table => new
                {
                    ResponseCacheId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Uri = table.Column<string>(type: "TEXT", nullable: true),
                    LastUpdated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Response = table.Column<string>(type: "TEXT", nullable: true),
                    StatusCode = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResponseCaches", x => x.ResponseCacheId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ResponseCaches_LastUpdated_StatusCode",
                table: "ResponseCaches",
                columns: new[] { "LastUpdated", "StatusCode" });

            migrationBuilder.CreateIndex(
                name: "IX_ResponseCaches_Uri",
                table: "ResponseCaches",
                column: "Uri",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ResponseCaches");
        }
    }
}
