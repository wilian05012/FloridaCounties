using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

namespace FloridaCounties.Migrations
{
    public partial class GeographyAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Polygon>(
                name: "Polygon",
                table: "tblCounties",
                nullable: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Polygon",
                table: "tblCounties");
        }
    }
}
