using Microsoft.EntityFrameworkCore.Migrations;

namespace ImageInfrastructure.Core.Migrations
{
    public partial class AddPostIdData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE ImageSources SET PostId = SUBSTR(PostUrl, 31) WHERE Source = 'Pixiv'");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
