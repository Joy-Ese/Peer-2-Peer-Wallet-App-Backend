using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WalletPayment.Services.Migrations
{
    public partial class MigNine : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "WalletUserAccount",
                table: "SystemTransactions",
                newName: "SystemAccount");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "SystemTransactions",
                newName: "TransactionType");

            migrationBuilder.AddColumn<decimal>(
                name: "ConversionRate",
                table: "SystemTransactions",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "KycImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ZippedImageName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TimeUploaded = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KycImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KycImages_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_KycImages_UserId",
                table: "KycImages",
                column: "UserId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KycImages");

            migrationBuilder.DropColumn(
                name: "ConversionRate",
                table: "SystemTransactions");

            migrationBuilder.RenameColumn(
                name: "TransactionType",
                table: "SystemTransactions",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "SystemAccount",
                table: "SystemTransactions",
                newName: "WalletUserAccount");
        }
    }
}
