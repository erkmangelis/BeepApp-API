using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BeepApp_API.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserTeamModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UpdateDate",
                table: "Teams",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "CreationDate",
                table: "Teams",
                newName: "CreatedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "Teams",
                newName: "UpdateDate");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Teams",
                newName: "CreationDate");
        }
    }
}
