using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tourism.Migrations
{
    /// <inheritdoc />
    public partial class v5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TourPlan_Trips_TripId",
                table: "TourPlan");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TourPlan",
                table: "TourPlan");

            migrationBuilder.RenameTable(
                name: "TourPlan",
                newName: "TourPlans");

            migrationBuilder.RenameIndex(
                name: "IX_TourPlan_TripId",
                table: "TourPlans",
                newName: "IX_TourPlans_TripId");

            migrationBuilder.AlterColumn<decimal>(
                name: "cost",
                table: "Trips",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TourPlans",
                table: "TourPlans",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TourPlans_Trips_TripId",
                table: "TourPlans",
                column: "TripId",
                principalTable: "Trips",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TourPlans_Trips_TripId",
                table: "TourPlans");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TourPlans",
                table: "TourPlans");

            migrationBuilder.RenameTable(
                name: "TourPlans",
                newName: "TourPlan");

            migrationBuilder.RenameIndex(
                name: "IX_TourPlans_TripId",
                table: "TourPlan",
                newName: "IX_TourPlan_TripId");

            migrationBuilder.AlterColumn<double>(
                name: "cost",
                table: "Trips",
                type: "double",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TourPlan",
                table: "TourPlan",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TourPlan_Trips_TripId",
                table: "TourPlan",
                column: "TripId",
                principalTable: "Trips",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
