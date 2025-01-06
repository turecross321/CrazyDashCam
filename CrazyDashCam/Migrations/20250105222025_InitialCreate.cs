using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrazyDashCam.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AmbientAirTemperatures",
                columns: table => new
                {
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Value = table.Column<float>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AmbientAirTemperatures", x => x.Timestamp);
                });

            migrationBuilder.CreateTable(
                name: "CoolantTemperatures",
                columns: table => new
                {
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Value = table.Column<float>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoolantTemperatures", x => x.Timestamp);
                });

            migrationBuilder.CreateTable(
                name: "EngineLoads",
                columns: table => new
                {
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Value = table.Column<float>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EngineLoads", x => x.Timestamp);
                });

            migrationBuilder.CreateTable(
                name: "FuelLevels",
                columns: table => new
                {
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Value = table.Column<float>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FuelLevels", x => x.Timestamp);
                });

            migrationBuilder.CreateTable(
                name: "IntakeTemperatures",
                columns: table => new
                {
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Value = table.Column<float>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntakeTemperatures", x => x.Timestamp);
                });

            migrationBuilder.CreateTable(
                name: "OilTemperatures",
                columns: table => new
                {
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Value = table.Column<float>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OilTemperatures", x => x.Timestamp);
                });

            migrationBuilder.CreateTable(
                name: "Rpms",
                columns: table => new
                {
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Value = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rpms", x => x.Timestamp);
                });

            migrationBuilder.CreateTable(
                name: "Speeds",
                columns: table => new
                {
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Value = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Speeds", x => x.Timestamp);
                });

            migrationBuilder.CreateTable(
                name: "ThrottlePositions",
                columns: table => new
                {
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Value = table.Column<float>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThrottlePositions", x => x.Timestamp);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AmbientAirTemperatures");

            migrationBuilder.DropTable(
                name: "CoolantTemperatures");

            migrationBuilder.DropTable(
                name: "EngineLoads");

            migrationBuilder.DropTable(
                name: "FuelLevels");

            migrationBuilder.DropTable(
                name: "IntakeTemperatures");

            migrationBuilder.DropTable(
                name: "OilTemperatures");

            migrationBuilder.DropTable(
                name: "Rpms");

            migrationBuilder.DropTable(
                name: "Speeds");

            migrationBuilder.DropTable(
                name: "ThrottlePositions");
        }
    }
}
