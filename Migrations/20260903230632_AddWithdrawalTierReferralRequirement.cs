using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EquityHarbour.Migrations
{
    /// <inheritdoc />
    public partial class AddWithdrawalTierReferralRequirement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MinReferralCount",
                table: "WithdrawalLimitTiers",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MinReferralCount",
                table: "WithdrawalLimitTiers");
        }
    }
}
