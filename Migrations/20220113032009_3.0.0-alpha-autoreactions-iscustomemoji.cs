using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tomoe.Migrations
{
    public partial class _300alphaautoreactionsiscustomemoji : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.AddColumn<bool>(
                name: "is_custom_emoji",
                table: "auto_reactions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

        protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropColumn(
                name: "is_custom_emoji",
                table: "auto_reactions");
    }
}
