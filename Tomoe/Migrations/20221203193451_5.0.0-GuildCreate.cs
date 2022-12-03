using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tomoe.Migrations
{
    /// <inheritdoc />
    public partial class _500GuildCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "guilds",
                columns: table => new
                {
                    id = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_guilds", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "members",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    userid = table.Column<decimal>(name: "user_id", type: "numeric(20,0)", nullable: false),
                    guildid = table.Column<decimal>(name: "guild_id", type: "numeric(20,0)", nullable: false),
                    joinedat = table.Column<DateTimeOffset>(name: "joined_at", type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_members", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "guilds");

            migrationBuilder.DropTable(
                name: "members");
        }
    }
}
