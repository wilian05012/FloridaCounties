using Microsoft.EntityFrameworkCore.Migrations;

namespace FloridaCounties.Migrations
{
    public partial class InitializeSchema : Migration
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
                    Name = table.Column<string>(maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblCounties", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tblCounties");
        }
    }
}
