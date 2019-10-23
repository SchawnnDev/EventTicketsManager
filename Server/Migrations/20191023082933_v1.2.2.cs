using Microsoft.EntityFrameworkCore.Migrations;

namespace Server.Migrations
{
    public partial class v122 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Sex",
                schema: "public",
                table: "Tickets");

            migrationBuilder.AddColumn<int>(
                name: "Gender",
                schema: "public",
                table: "Tickets",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Gender",
                schema: "public",
                table: "Tickets");

            migrationBuilder.AddColumn<bool>(
                name: "Sex",
                schema: "public",
                table: "Tickets",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
