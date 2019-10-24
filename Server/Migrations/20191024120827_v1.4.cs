using Microsoft.EntityFrameworkCore.Migrations;

namespace Server.Migrations
{
    public partial class v14 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatorId",
                schema: "public",
                table: "TicketUserMails",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatorId",
                schema: "public",
                table: "TicketScans",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatorId",
                schema: "public",
                table: "TicketUserMails");

            migrationBuilder.DropColumn(
                name: "CreatorId",
                schema: "public",
                table: "TicketScans");
        }
    }
}
