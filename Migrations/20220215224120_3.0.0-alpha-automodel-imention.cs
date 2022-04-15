using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tomoe.Migrations
{
    public partial class _300alphaautomodelimention : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "emoji_data");

            migrationBuilder.DropTable(
                name: "guild_configs");

            migrationBuilder.DropTable(
                name: "snowflake_perms");

            migrationBuilder.DropTable(
                name: "tags");

            migrationBuilder.DropTable(
                name: "auto_reactions");

            migrationBuilder.DropColumn(
                name: "is_role",
                table: "auto_mentions");

            migrationBuilder.DropColumn(
                name: "snowflake",
                table: "auto_mentions");

            migrationBuilder.RenameColumn(
                name: "regex",
                table: "auto_mentions",
                newName: "filter");

            migrationBuilder.AddColumn<int>(
                name: "filter_type",
                table: "auto_mentions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<List<string>>(
                name: "values",
                table: "auto_mentions",
                type: "text[]",
                nullable: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "filter_type",
                table: "auto_mentions");

            migrationBuilder.DropColumn(
                name: "values",
                table: "auto_mentions");

            migrationBuilder.RenameColumn(
                name: "filter",
                table: "auto_mentions",
                newName: "regex");

            migrationBuilder.AddColumn<bool>(
                name: "is_role",
                table: "auto_mentions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "snowflake",
                table: "auto_mentions",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "auto_reactions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    channel_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    filter_type = table.Column<int>(type: "integer", nullable: false),
                    guild_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    regex = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table => table.PrimaryKey("pk_auto_reactions", x => x.id));

            migrationBuilder.CreateTable(
                name: "guild_configs",
                columns: table => new
                {
                    guild_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table => table.PrimaryKey("pk_guild_configs", x => x.guild_id));

            migrationBuilder.CreateTable(
                name: "snowflake_perms",
                columns: table => new
                {
                    snowflake_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    guild_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    permissions = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table => table.PrimaryKey("pk_snowflake_perms", x => x.snowflake_id));

            migrationBuilder.CreateTable(
                name: "tags",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    aliases = table.Column<List<string>>(type: "text[]", nullable: false),
                    author_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    guild_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    usage_count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table => table.PrimaryKey("pk_tags", x => x.id));

            migrationBuilder.CreateTable(
                name: "emoji_data",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    auto_reaction_model_id = table.Column<Guid>(type: "uuid", nullable: true),
                    emoji_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    emoji_name = table.Column<string>(type: "text", nullable: false)
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
    }
}
