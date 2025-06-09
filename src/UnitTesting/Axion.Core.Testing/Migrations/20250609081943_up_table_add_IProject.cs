using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Andux.Core.Testing.Migrations
{
    /// <inheritdoc />
    public partial class up_table_add_IProject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ProjectId",
                table: "User",
                type: "bigint",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "User");
        }
    }
}
