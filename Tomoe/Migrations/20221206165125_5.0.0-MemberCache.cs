using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tomoe.Migrations
{
    /// <inheritdoc />
    public partial class _500MemberCache : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "flags",
                table: "members",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "flags",
                table: "members");
        }
    }
}
