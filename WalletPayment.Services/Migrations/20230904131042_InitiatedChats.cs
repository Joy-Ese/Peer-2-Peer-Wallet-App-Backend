using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WalletPayment.Services.Migrations
{
    public partial class InitiatedChats : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InitiatedChats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IsChatInitiated = table.Column<bool>(type: "bit", nullable: false),
                    StartedWith = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReceivedFrom = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InitiatedChats", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InitiatedChats");
        }
    }
}
