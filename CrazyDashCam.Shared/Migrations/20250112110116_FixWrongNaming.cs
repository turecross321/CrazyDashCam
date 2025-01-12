using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrazyDashCam.Shared.Migrations
{
    /// <inheritdoc />
    public partial class FixWrongNaming : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Timestamp",
                table: "ThrottlePositions",
                newName: "Date");

            migrationBuilder.RenameColumn(
                name: "Timestamp",
                table: "Speeds",
                newName: "Date");

            migrationBuilder.RenameColumn(
                name: "Timestamp",
                table: "Rpms",
                newName: "Date");

            migrationBuilder.RenameColumn(
                name: "Timestamp",
                table: "OilTemperatures",
                newName: "Date");

            migrationBuilder.RenameColumn(
                name: "Timestamp",
                table: "Locations",
                newName: "Date");

            migrationBuilder.RenameColumn(
                name: "Timestamp",
                table: "IntakeTemperatures",
                newName: "Date");

            migrationBuilder.RenameColumn(
                name: "Timestamp",
                table: "FuelLevels",
                newName: "Date");

            migrationBuilder.RenameColumn(
                name: "Timestamp",
                table: "EngineLoads",
                newName: "Date");

            migrationBuilder.RenameColumn(
                name: "Timestamp",
                table: "CoolantTemperatures",
                newName: "Date");

            migrationBuilder.RenameColumn(
                name: "Timestamp",
                table: "AmbientAirTemperatures",
                newName: "Date");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Date",
                table: "ThrottlePositions",
                newName: "Timestamp");

            migrationBuilder.RenameColumn(
                name: "Date",
                table: "Speeds",
                newName: "Timestamp");

            migrationBuilder.RenameColumn(
                name: "Date",
                table: "Rpms",
                newName: "Timestamp");

            migrationBuilder.RenameColumn(
                name: "Date",
                table: "OilTemperatures",
                newName: "Timestamp");

            migrationBuilder.RenameColumn(
                name: "Date",
                table: "Locations",
                newName: "Timestamp");

            migrationBuilder.RenameColumn(
                name: "Date",
                table: "IntakeTemperatures",
                newName: "Timestamp");

            migrationBuilder.RenameColumn(
                name: "Date",
                table: "FuelLevels",
                newName: "Timestamp");

            migrationBuilder.RenameColumn(
                name: "Date",
                table: "EngineLoads",
                newName: "Timestamp");

            migrationBuilder.RenameColumn(
                name: "Date",
                table: "CoolantTemperatures",
                newName: "Timestamp");

            migrationBuilder.RenameColumn(
                name: "Date",
                table: "AmbientAirTemperatures",
                newName: "Timestamp");
        }
    }
}
