using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WalletPayment.Services.Migrations
{
    public partial class KycDocsEditChatAndDataContextToo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SignalrConnections");

            migrationBuilder.DropColumn(
                name: "ChatType",
                table: "Chats");

            migrationBuilder.AddColumn<string>(
                name: "FileCode",
                table: "KycImages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "AdminUserId",
                table: "Chats",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "KycDocuments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KycDocuments", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Chats_AdminUserId",
                table: "Chats",
                column: "AdminUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chats_Adminss_AdminUserId",
                table: "Chats",
                column: "AdminUserId",
                principalTable: "Adminss",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chats_Adminss_AdminUserId",
                table: "Chats");

            migrationBuilder.DropTable(
                name: "KycDocuments");

            migrationBuilder.DropIndex(
                name: "IX_Chats_AdminUserId",
                table: "Chats");

            migrationBuilder.DropColumn(
                name: "FileCode",
                table: "KycImages");

            migrationBuilder.DropColumn(
                name: "AdminUserId",
                table: "Chats");

            migrationBuilder.AddColumn<string>(
                name: "ChatType",
                table: "Chats",
                type: "varchar(50)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "SignalrConnections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdminUserId = table.Column<int>(type: "int", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    SignalrId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SignalrConnections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SignalrConnections_Adminss_AdminUserId",
                        column: x => x.AdminUserId,
                        principalTable: "Adminss",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SignalrConnections_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SignalrConnections_AdminUserId",
                table: "SignalrConnections",
                column: "AdminUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SignalrConnections_UserId",
                table: "SignalrConnections",
                column: "UserId");
        }
    }
}
