using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MoviesReservationSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentIdToReservations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PaymentId",
                table: "MovieReservations",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentId",
                table: "MovieReservations");
        }
    }
}
