using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WalletPayment.Services.Migrations
{
    public partial class UserToUserChatEdit : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserToUserChats_Users_RecipientUserId",
                table: "UserToUserChats");

            migrationBuilder.DropForeignKey(
                name: "FK_UserToUserChats_Users_SenderUserId",
                table: "UserToUserChats");

            migrationBuilder.DropIndex(
                name: "IX_UserToUserChats_RecipientUserId",
                table: "UserToUserChats");

            migrationBuilder.DropIndex(
                name: "IX_UserToUserChats_SenderUserId",
                table: "UserToUserChats");

            migrationBuilder.DropColumn(
                name: "RecipientUserId",
                table: "UserToUserChats");

            migrationBuilder.DropColumn(
                name: "SenderUserId",
                table: "UserToUserChats");

            migrationBuilder.AddColumn<string>(
                name: "RecipientUsername",
                table: "UserToUserChats",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SenderUsernmae",
                table: "UserToUserChats",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RecipientUsername",
                table: "UserToUserChats");

            migrationBuilder.DropColumn(
                name: "SenderUsernmae",
                table: "UserToUserChats");

            migrationBuilder.AddColumn<int>(
                name: "RecipientUserId",
                table: "UserToUserChats",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SenderUserId",
                table: "UserToUserChats",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_UserToUserChats_RecipientUserId",
                table: "UserToUserChats",
                column: "RecipientUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserToUserChats_SenderUserId",
                table: "UserToUserChats",
                column: "SenderUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserToUserChats_Users_RecipientUserId",
                table: "UserToUserChats",
                column: "RecipientUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserToUserChats_Users_SenderUserId",
                table: "UserToUserChats",
                column: "SenderUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
