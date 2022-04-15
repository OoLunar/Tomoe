using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tomoe.Migrations
{
    public partial class _300alphaautoreactions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.CreateTable(
                name: "auto_reactions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    guild_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    channel_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    emoji_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    emoji_name = table.Column<string>(type: "text", nullable: false),
                    animated = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table => table.PrimaryKey("pk_auto_reactions", x => x.id));

        protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropTable(
                name: "auto_reactions");
    }
}
