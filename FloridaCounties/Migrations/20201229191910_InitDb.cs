using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

namespace FloridaCounties.Migrations
{
    public partial class InitDb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tblCounties",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    DepCode = table.Column<int>(nullable: false),
                    EsriId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(maxLength: 50, nullable: false),
                    Shape = table.Column<MultiPolygon>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblCounties", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tblCities",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    PlaceFP = table.Column<int>(nullable: false),
                    BebrId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Notes = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    EntryCreationDate = table.Column<DateTime>(nullable: false),
                    Area = table.Column<double>(nullable: false),
                    Perimeter = table.Column<double>(nullable: false),
                    Shape = table.Column<MultiPolygon>(nullable: false),
                    CountyId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblCities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tblCities_tblCounties_CountyId",
                        column: x => x.CountyId,
                        principalTable: "tblCounties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tblCities_CountyId",
                table: "tblCities",
                column: "CountyId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tblCities");

            migrationBuilder.DropTable(
                name: "tblCounties");
        }
    }
}
