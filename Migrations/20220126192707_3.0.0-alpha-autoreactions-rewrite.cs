using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tomoe.Migrations
{
    public partial class _300alphaautoreactionsrewrite : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "animated",
                table: "auto_reactions");

            migrationBuilder.DropColumn(
                name: "emoji_id",
                table: "auto_reactions");

            migrationBuilder.DropColumn(
                name: "emoji_name",
                table: "auto_reactions");

            migrationBuilder.DropColumn(
                name: "is_custom_emoji",
                table: "auto_reactions");

            migrationBuilder.AddColumn<int>(
                name: "filter_type",
                table: "auto_reactions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "regex",
                table: "auto_reactions",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "emoji_data",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    emoji_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    emoji_name = table.Column<string>(type: "text", nullable: false),
                    auto_reaction_model_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_emoji_data", x => x.id);
                    table.ForeignKey(
                        name: "fk_emoji_data_auto_reactions_auto_reaction_model_id",
                        column: x => x.auto_reaction_model_id,
                        principalTable: "auto_reactions",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "ix_emoji_data_auto_reaction_model_id",
                table: "emoji_data",
                column: "auto_reaction_model_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "emoji_data");

            migrationBuilder.DropColumn(
                name: "filter_type",
                table: "auto_reactions");

            migrationBuilder.DropColumn(
                name: "regex",
                table: "auto_reactions");

            migrationBuilder.AddColumn<bool>(
                name: "animated",
                table: "auto_reactions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "emoji_id",
                table: "auto_reactions",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "emoji_name",
                table: "auto_reactions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "is_custom_emoji",
                table: "auto_reactions",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
