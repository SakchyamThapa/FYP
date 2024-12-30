using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MVC.Migrations
{
    /// <inheritdoc />
    public partial class update04 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RedeemModelId",
                table: "Redeems",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Redeems_RedeemModelId",
                table: "Redeems",
                column: "RedeemModelId");

            migrationBuilder.AddForeignKey(
                name: "FK_Redeems_Redeems_RedeemModelId",
                table: "Redeems",
                column: "RedeemModelId",
                principalTable: "Redeems",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Redeems_Redeems_RedeemModelId",
                table: "Redeems");

            migrationBuilder.DropIndex(
                name: "IX_Redeems_RedeemModelId",
                table: "Redeems");

            migrationBuilder.DropColumn(
                name: "RedeemModelId",
                table: "Redeems");
        }
    }
}
