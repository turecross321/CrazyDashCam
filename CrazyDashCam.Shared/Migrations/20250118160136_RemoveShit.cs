using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrazyDashCam.Shared.Migrations
{
    /// <inheritdoc />
    public partial class RemoveShit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AbsoluteLoads");

            migrationBuilder.DropTable(
                name: "CalculatedEngineLoads");

            migrationBuilder.DropTable(
                name: "IntakeTemperatures");

            migrationBuilder.DropTable(
                name: "RelativeThrottlePositions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AbsoluteLoads",
                columns: table => new
                {
                    Date = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    Value = table.Column<float>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbsoluteLoads", x => x.Date);
                });

            migrationBuilder.CreateTable(
                name: "CalculatedEngineLoads",
                columns: table => new
                {
                    Date = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    Value = table.Column<float>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalculatedEngineLoads", x => x.Date);
                });

            migrationBuilder.CreateTable(
                name: "IntakeTemperatures",
                columns: table => new
                {
                    Date = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    Value = table.Column<float>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntakeTemperatures", x => x.Date);
                });

            migrationBuilder.CreateTable(
                name: "RelativeThrottlePositions",
                columns: table => new
                {
                    Date = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    Value = table.Column<float>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RelativeThrottlePositions", x => x.Date);
                });
        }
    }
}
