using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EquityHarbour.Migrations
{
    /// <inheritdoc />
    public partial class AddPayoutLockingAndWithdrawalTiers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "LockedBalance",
                table: "Wallets",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "UnlockAt",
                table: "InvestmentPayouts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Unlocked",
                table: "InvestmentPayouts",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "WithdrawalLimitTiers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MinInvestedAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    MaxInvestedAmount = table.Column<decimal>(type: "numeric", nullable: true),
                    MinWithdrawalAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    MaxWithdrawalAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WithdrawalLimitTiers", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WithdrawalLimitTiers");

            migrationBuilder.DropColumn(
                name: "LockedBalance",
                table: "Wallets");

            migrationBuilder.DropColumn(
                name: "UnlockAt",
                table: "InvestmentPayouts");

            migrationBuilder.DropColumn(
                name: "Unlocked",
                table: "InvestmentPayouts");
        }
    }
}
