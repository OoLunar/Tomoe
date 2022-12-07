using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tomoe.Migrations
{
    /// <inheritdoc />
    public partial class _500polls : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:hstore", ",,");

            migrationBuilder.CreateTable(
                name: "polls",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    question = table.Column<string>(type: "text", nullable: false),
                    options = table.Column<Dictionary<string, string>>(type: "jsonb", nullable: false),
                    votes = table.Column<Dictionary<ulong, int>>(type: "json", nullable: false),
                    expiresat = table.Column<DateTimeOffset>(name: "expires_at", type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_polls", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "polls");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:PostgresExtension:hstore", ",,");
        }
    }
}
