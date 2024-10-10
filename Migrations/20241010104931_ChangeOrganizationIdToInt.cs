using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BeepApp_API.Migrations
{
    /// <inheritdoc />
    public partial class ChangeOrganizationIdToInt : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop the old OrganizationId column
            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "Tests"); // Replace with your actual table name

            // Add the new OrganizationId column with the desired type
            migrationBuilder.AddColumn<int>(
                name: "OrganizationId",
                table: "Tests", // Replace with your actual table name
                nullable: false,
                defaultValue: 0);

            // Drop the old OrganizationId column
            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "DOTests"); // Replace with your actual table name

            // Add the new OrganizationId column with the desired type
            migrationBuilder.AddColumn<int>(
                name: "OrganizationId",
                table: "DOTests", // Replace with your actual table name
                nullable: false,
                defaultValue: 0);
        }
    }
}
