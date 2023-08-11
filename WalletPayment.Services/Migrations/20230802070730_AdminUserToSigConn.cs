using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WalletPayment.Services.Migrations
{
    public partial class AdminUserToSigConn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AdminUserId",
                table: "SignalrConnections",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SignalrConnections_AdminUserId",
                table: "SignalrConnections",
                column: "AdminUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_SignalrConnections_Adminss_AdminUserId",
                table: "SignalrConnections",
                column: "AdminUserId",
                principalTable: "Adminss",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SignalrConnections_Adminss_AdminUserId",
                table: "SignalrConnections");

            migrationBuilder.DropIndex(
                name: "IX_SignalrConnections_AdminUserId",
                table: "SignalrConnections");

            migrationBuilder.DropColumn(
                name: "AdminUserId",
                table: "SignalrConnections");
        }
    }
}
