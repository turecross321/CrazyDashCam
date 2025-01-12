using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrazyDashCam.Shared.Migrations
{
    /// <inheritdoc />
    public partial class AddRelativeThrottlePos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RelativeThrottlePositions");
        }
    }
}
