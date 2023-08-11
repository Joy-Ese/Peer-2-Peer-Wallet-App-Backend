using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WalletPayment.Services.Migrations
{
    public partial class UpdateDataContext : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Users_UserId",
                table: "Chat");

            migrationBuilder.DropForeignKey(
                name: "FK_SignalrConnection_Users_UserId",
                table: "SignalrConnection");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SignalrConnection",
                table: "SignalrConnection");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Chat",
                table: "Chat");

            migrationBuilder.RenameTable(
                name: "SignalrConnection",
                newName: "SignalrConnections");

            migrationBuilder.RenameTable(
                name: "Chat",
                newName: "Chats");

            migrationBuilder.RenameIndex(
                name: "IX_SignalrConnection_UserId",
                table: "SignalrConnections",
                newName: "IX_SignalrConnections_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Chat_UserId",
                table: "Chats",
                newName: "IX_Chats_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SignalrConnections",
                table: "SignalrConnections",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Chats",
                table: "Chats",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Chats_Users_UserId",
                table: "Chats",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SignalrConnections_Users_UserId",
                table: "SignalrConnections",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chats_Users_UserId",
                table: "Chats");

            migrationBuilder.DropForeignKey(
                name: "FK_SignalrConnections_Users_UserId",
                table: "SignalrConnections");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SignalrConnections",
                table: "SignalrConnections");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Chats",
                table: "Chats");

            migrationBuilder.RenameTable(
                name: "SignalrConnections",
                newName: "SignalrConnection");

            migrationBuilder.RenameTable(
                name: "Chats",
                newName: "Chat");

            migrationBuilder.RenameIndex(
                name: "IX_SignalrConnections_UserId",
                table: "SignalrConnection",
                newName: "IX_SignalrConnection_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Chats_UserId",
                table: "Chat",
                newName: "IX_Chat_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SignalrConnection",
                table: "SignalrConnection",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Chat",
                table: "Chat",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Users_UserId",
                table: "Chat",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SignalrConnection_Users_UserId",
                table: "SignalrConnection",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
