using Microsoft.EntityFrameworkCore.Migrations;

namespace ImageInfrastructure.Core.Migrations
{
    public partial class AddSizeAndRelated : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ImageTags_Name_Safety_Type",
                table: "ImageTags");

            migrationBuilder.DropIndex(
                name: "IX_ImageSources_Uri_Source",
                table: "ImageSources");

            migrationBuilder.AddColumn<int>(
                name: "Height",
                table: "Images",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Width",
                table: "Images",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "RelatedImages",
                columns: table => new
                {
                    RelatedImageId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ImageId = table.Column<int>(type: "INTEGER", nullable: true),
                    ImageSourceId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RelatedImages", x => x.RelatedImageId);
                    table.ForeignKey(
                        name: "FK_RelatedImages_Images_ImageId",
                        column: x => x.ImageId,
                        principalTable: "Images",
                        principalColumn: "ImageId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RelatedImages_ImageSources_ImageSourceId",
                        column: x => x.ImageSourceId,
                        principalTable: "ImageSources",
                        principalColumn: "ImageSourceId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RelatedImages_ImageId",
                table: "RelatedImages",
                column: "ImageId");

            migrationBuilder.CreateIndex(
                name: "IX_RelatedImages_ImageSourceId",
                table: "RelatedImages",
                column: "ImageSourceId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RelatedImages");

            migrationBuilder.DropColumn(
                name: "Height",
                table: "Images");

            migrationBuilder.DropColumn(
                name: "Width",
                table: "Images");

            migrationBuilder.CreateIndex(
                name: "IX_ImageTags_Name_Safety_Type",
                table: "ImageTags",
                columns: new[] { "Name", "Safety", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_ImageSources_Uri_Source",
                table: "ImageSources",
                columns: new[] { "Uri", "Source" });
        }
    }
}
