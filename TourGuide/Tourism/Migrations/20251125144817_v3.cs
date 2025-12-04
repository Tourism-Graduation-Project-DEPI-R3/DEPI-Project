using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tourism.Migrations
{
    /// <inheritdoc />
    public partial class v3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "dateAdded",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "endDate",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "verificationDocuments",
                table: "TourGuides");

            migrationBuilder.RenameColumn(
                name: "startDate",
                table: "Trips",
                newName: "Duration");

            migrationBuilder.AddColumn<byte[]>(
                name: "MainImage",
                table: "Trips",
                type: "longblob",
                nullable: false);

            migrationBuilder.AddColumn<string>(
                name: "TripSecondaryImagesJson",
                table: "Trips",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "triptype",
                table: "Trips",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<byte[]>(
                name: "verificationDocuments",
                table: "Trips",
                type: "longblob",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TourPlan",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Heading = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TripId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TourPlan", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TourPlan_Trips_TripId",
                        column: x => x.TripId,
                        principalTable: "Trips",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_TourPlan_TripId",
                table: "TourPlan",
                column: "TripId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TourPlan");

            migrationBuilder.DropColumn(
                name: "MainImage",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "TripSecondaryImagesJson",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "triptype",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "verificationDocuments",
                table: "Trips");

            migrationBuilder.RenameColumn(
                name: "Duration",
                table: "Trips",
                newName: "startDate");

            migrationBuilder.AddColumn<DateTime>(
                name: "dateAdded",
                table: "Trips",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "endDate",
                table: "Trips",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<byte[]>(
                name: "verificationDocuments",
                table: "TourGuides",
                type: "longblob",
                nullable: true);
        }
    }
}
