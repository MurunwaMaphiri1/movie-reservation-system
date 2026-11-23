using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MoviesReservationSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddGuestDetailsToReservations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MovieReservations_Users_UserId",
                table: "MovieReservations");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "MovieReservations",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "MovieReservations",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "MovieReservations",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_MovieReservations_Users_UserId",
                table: "MovieReservations",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MovieReservations_Users_UserId",
                table: "MovieReservations");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "MovieReservations");

            migrationBuilder.DropColumn(
                name: "FullName",
                table: "MovieReservations");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "MovieReservations",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_MovieReservations_Users_UserId",
                table: "MovieReservations",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
