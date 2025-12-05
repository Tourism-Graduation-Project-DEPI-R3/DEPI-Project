using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tourism.Migrations
{
    /// <inheritdoc />
    public partial class MakeTourGuideCreditCardNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TourGuides_CreditCards_creditCardCardNumber",
                table: "TourGuides");

            migrationBuilder.AlterColumn<string>(
                name: "creditCardCardNumber",
                table: "TourGuides",
                type: "nvarchar(16)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(16)");

            migrationBuilder.AddForeignKey(
                name: "FK_TourGuides_CreditCards_creditCardCardNumber",
                table: "TourGuides",
                column: "creditCardCardNumber",
                principalTable: "CreditCards",
                principalColumn: "CardNumber");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TourGuides_CreditCards_creditCardCardNumber",
                table: "TourGuides");

            migrationBuilder.AlterColumn<string>(
                name: "creditCardCardNumber",
                table: "TourGuides",
                type: "nvarchar(16)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(16)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TourGuides_CreditCards_creditCardCardNumber",
                table: "TourGuides",
                column: "creditCardCardNumber",
                principalTable: "CreditCards",
                principalColumn: "CardNumber",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
