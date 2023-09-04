using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WalletPayment.Services.Migrations
{
    public partial class UserToUserChatEdit1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserToUserChats_Users_UserId",
                table: "UserToUserChats");

            migrationBuilder.DropIndex(
                name: "IX_UserToUserChats_UserId",
                table: "UserToUserChats");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "UserToUserChats");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "UserToUserChats",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserToUserChats_UserId",
                table: "UserToUserChats",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserToUserChats_Users_UserId",
                table: "UserToUserChats",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
