using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tomoe.Migrations
{
    public partial class _300AlphaAutoMentions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "auto_mentions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    guild_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    channel_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    snowflake = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    is_role = table.Column<bool>(type: "boolean", nullable: false),
                    regex = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_auto_mentions", x => x.id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "auto_mentions");
        }
    }
}
