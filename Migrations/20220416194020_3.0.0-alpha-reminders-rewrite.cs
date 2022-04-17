using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tomoe.Migrations
{
    public partial class _300alpharemindersrewrite : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "channel_id",
                table: "reminders");

            migrationBuilder.DropColumn(
                name: "guild_id",
                table: "reminders");

            migrationBuilder.AddColumn<string>(
                name: "message_link",
                table: "reminders",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "reply",
                table: "reminders",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "message_link",
                table: "reminders");

            migrationBuilder.DropColumn(
                name: "reply",
                table: "reminders");

            migrationBuilder.AddColumn<decimal>(
                name: "channel_id",
                table: "reminders",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "guild_id",
                table: "reminders",
                type: "numeric(20,0)",
                nullable: true);
        }
    }
}
