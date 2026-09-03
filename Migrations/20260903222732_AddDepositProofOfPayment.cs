using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EquityHarbour.Migrations
{
    /// <inheritdoc />
    public partial class AddDepositProofOfPayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProofImagePath",
                table: "Deposits",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserProvidedReference",
                table: "Deposits",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProofImagePath",
                table: "Deposits");

            migrationBuilder.DropColumn(
                name: "UserProvidedReference",
                table: "Deposits");
        }
    }
}
