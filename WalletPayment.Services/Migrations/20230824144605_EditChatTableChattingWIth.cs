using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WalletPayment.Services.Migrations
{
    public partial class EditChatTableChattingWIth : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ChattingWith",
                table: "Chats",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChattingWith",
                table: "Chats");
        }
    }
}
