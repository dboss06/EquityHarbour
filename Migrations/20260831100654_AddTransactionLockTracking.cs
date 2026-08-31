using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EquityHarbour.Migrations
{
    /// <inheritdoc />
    public partial class AddTransactionLockTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsLocked",
                table: "WalletTransactions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UnlockAt",
                table: "WalletTransactions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Reference",
                table: "InvestmentPayouts",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsLocked",
                table: "WalletTransactions");

            migrationBuilder.DropColumn(
                name: "UnlockAt",
                table: "WalletTransactions");

            migrationBuilder.DropColumn(
                name: "Reference",
                table: "InvestmentPayouts");
        }
    }
}
