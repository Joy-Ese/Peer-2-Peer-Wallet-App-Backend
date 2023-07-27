using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WalletPayment.Services.Migrations
{
    public partial class MigTwelve : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_KycImages_UserId",
                table: "KycImages");

            migrationBuilder.CreateIndex(
                name: "IX_KycImages_UserId",
                table: "KycImages",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_KycImages_UserId",
                table: "KycImages");

            migrationBuilder.CreateIndex(
                name: "IX_KycImages_UserId",
                table: "KycImages",
                column: "UserId",
                unique: true);
        }
    }
}
