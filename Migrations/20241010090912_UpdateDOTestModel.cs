using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BeepApp_API.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDOTestModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlayerTeams_Players_PlayerId",
                table: "PlayerTeams");

            migrationBuilder.DropForeignKey(
                name: "FK_PlayerTeams_Teams_TeamId",
                table: "PlayerTeams");

            migrationBuilder.DropIndex(
                name: "IX_PlayerTeams_PlayerId",
                table: "PlayerTeams");

            migrationBuilder.DropIndex(
                name: "IX_PlayerTeams_TeamId",
                table: "PlayerTeams");

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                table: "DOTests",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "DOTests");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerTeams_PlayerId",
                table: "PlayerTeams",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerTeams_TeamId",
                table: "PlayerTeams",
                column: "TeamId");

            migrationBuilder.AddForeignKey(
                name: "FK_PlayerTeams_Players_PlayerId",
                table: "PlayerTeams",
                column: "PlayerId",
                principalTable: "Players",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PlayerTeams_Teams_TeamId",
                table: "PlayerTeams",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
