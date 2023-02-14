using Microsoft.EntityFrameworkCore.Migrations;

namespace unbooru.Core.Migrations
{
    public partial class AddImageComposition : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ImageComposition",
                columns: table => new
                {
                    ImageCompositionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ImageId = table.Column<int>(type: "int", nullable: false),
                    IsGrayscale = table.Column<bool>(type: "bit", nullable: false),
                    IsBlackAndWhite = table.Column<bool>(type: "bit", nullable: false),
                    IsMonochrome = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImageComposition", x => x.ImageCompositionId);
                    table.ForeignKey(
                        name: "FK_ImageComposition_Images_ImageId",
                        column: x => x.ImageId,
                        principalTable: "Images",
                        principalColumn: "ImageId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ImageHistogramColor",
                columns: table => new
                {
                    ImageHistogramColorId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ColorKey = table.Column<long>(type: "bigint", nullable: false),
                    Value = table.Column<int>(type: "int", nullable: false),
                    CompositionImageCompositionId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImageHistogramColor", x => x.ImageHistogramColorId);
                    table.ForeignKey(
                        name: "FK_ImageHistogramColor_ImageComposition_CompositionImageCompositionId",
                        column: x => x.CompositionImageCompositionId,
                        principalTable: "ImageComposition",
                        principalColumn: "ImageCompositionId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ImageComposition_ImageId",
                table: "ImageComposition",
                column: "ImageId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ImageHistogramColor_ColorKey",
                table: "ImageHistogramColor",
                column: "ColorKey");

            migrationBuilder.CreateIndex(
                name: "IX_ImageHistogramColor_CompositionImageCompositionId",
                table: "ImageHistogramColor",
                column: "CompositionImageCompositionId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ImageHistogramColor");

            migrationBuilder.DropTable(
                name: "ImageComposition");
        }
    }
}
