using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tomoe.Migrations
{
    /// <inheritdoc />
    public partial class _500MemberRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal[]>(
                name: "role_ids",
                table: "members",
                type: "numeric(20,0)[]",
                nullable: false,
                defaultValue: new decimal[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "role_ids",
                table: "members");
        }
    }
}
