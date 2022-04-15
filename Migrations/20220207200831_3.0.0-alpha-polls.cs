using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tomoe.Migrations
{
    public partial class _300alphapolls : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.CreateTable(
                name: "polls",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    guild_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    channel_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    user_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    question = table.Column<string>(type: "text", nullable: false),
                    timeout = table.Column<TimeSpan>(type: "interval", nullable: false),
                    votes = table.Column<Dictionary<string, int>>(type: "jsonb", nullable: false)
                },
                constraints: table => table.PrimaryKey("pk_polls", x => x.id));

        protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropTable(
                name: "polls");
    }
}
