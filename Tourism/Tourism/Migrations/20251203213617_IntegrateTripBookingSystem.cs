using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tourism.Migrations
{
    /// <inheritdoc />
    public partial class IntegrateTripBookingSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "endDate",
                table: "Trips");

            migrationBuilder.RenameColumn(
                name: "startDate",
                table: "Trips",
                newName: "StartDate");

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "Trips",
                type: "nvarchar(max)",
                maxLength: 5000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<decimal>(
                name: "cost",
                table: "Trips",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AddColumn<int>(
                name: "Capacity",
                table: "Trips",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Duration",
                table: "Trips",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<byte[]>(
                name: "MainImage",
                table: "Trips",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<int>(
                name: "RemainingSeats",
                table: "Trips",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "TripSecondaryImagesJson",
                table: "Trips",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "triptype",
                table: "Trips",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "TourGuideId",
                table: "CreditCards",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "CreditCards",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PaymentTripBookings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TripId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    BookingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentTripBookings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentTripBookings_Trips_TripId",
                        column: x => x.TripId,
                        principalTable: "Trips",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TouristCarts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TouristId = table.Column<int>(type: "int", nullable: false),
                    TripId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TouristCarts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TouristCarts_Tourists_TouristId",
                        column: x => x.TouristId,
                        principalTable: "Tourists",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TouristCarts_Trips_TripId",
                        column: x => x.TripId,
                        principalTable: "Trips",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TourPlans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Heading = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TripId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TourPlans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TourPlans_Trips_TripId",
                        column: x => x.TripId,
                        principalTable: "Trips",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CreditCards_TourGuideId",
                table: "CreditCards",
                column: "TourGuideId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTripBookings_TripId",
                table: "PaymentTripBookings",
                column: "TripId");

            migrationBuilder.CreateIndex(
                name: "IX_TouristCarts_TouristId",
                table: "TouristCarts",
                column: "TouristId");

            migrationBuilder.CreateIndex(
                name: "IX_TouristCarts_TripId",
                table: "TouristCarts",
                column: "TripId");

            migrationBuilder.CreateIndex(
                name: "IX_TourPlans_TripId",
                table: "TourPlans",
                column: "TripId");

            migrationBuilder.AddForeignKey(
                name: "FK_CreditCards_TourGuides_TourGuideId",
                table: "CreditCards",
                column: "TourGuideId",
                principalTable: "TourGuides",
                principalColumn: "TourGuideId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CreditCards_TourGuides_TourGuideId",
                table: "CreditCards");

            migrationBuilder.DropTable(
                name: "PaymentTripBookings");

            migrationBuilder.DropTable(
                name: "TouristCarts");

            migrationBuilder.DropTable(
                name: "TourPlans");

            migrationBuilder.DropIndex(
                name: "IX_CreditCards_TourGuideId",
                table: "CreditCards");

            migrationBuilder.DropColumn(
                name: "Capacity",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "Duration",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "MainImage",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "RemainingSeats",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "TripSecondaryImagesJson",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "triptype",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "TourGuideId",
                table: "CreditCards");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "CreditCards");

            migrationBuilder.RenameColumn(
                name: "StartDate",
                table: "Trips",
                newName: "startDate");

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "Trips",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldMaxLength: 5000);

            migrationBuilder.AlterColumn<double>(
                name: "cost",
                table: "Trips",
                type: "float",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AddColumn<DateTime>(
                name: "endDate",
                table: "Trips",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
