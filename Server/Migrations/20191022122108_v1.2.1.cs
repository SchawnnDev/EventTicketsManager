using Microsoft.EntityFrameworkCore.Migrations;

namespace Server.Migrations
{
    public partial class v121 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventUsers_AspNetUsers_UserId",
                schema: "public",
                table: "EventUsers");

            migrationBuilder.DropIndex(
                name: "IX_EventUsers_UserId",
                schema: "public",
                table: "EventUsers");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_EventUsers_UserId",
                schema: "public",
                table: "EventUsers",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_EventUsers_AspNetUsers_UserId",
                schema: "public",
                table: "EventUsers",
                column: "UserId",
                principalSchema: "public",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
