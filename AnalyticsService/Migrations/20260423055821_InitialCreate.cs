using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AnalyticsService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OccupancyLogs",
                columns: table => new
                {
                    LogId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LotId = table.Column<int>(type: "integer", nullable: false),
                    SpotId = table.Column<int>(type: "integer", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    OccupancyRate = table.Column<double>(type: "double precision", nullable: false),
                    AvailableSpots = table.Column<int>(type: "integer", nullable: false),
                    TotalSpots = table.Column<int>(type: "integer", nullable: false),
                    VehicleType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OccupancyLogs", x => x.LogId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OccupancyLogs_LotId",
                table: "OccupancyLogs",
                column: "LotId");

            migrationBuilder.CreateIndex(
                name: "IX_OccupancyLogs_LotId_Timestamp",
                table: "OccupancyLogs",
                columns: new[] { "LotId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_OccupancyLogs_SpotId",
                table: "OccupancyLogs",
                column: "SpotId");

            migrationBuilder.CreateIndex(
                name: "IX_OccupancyLogs_Timestamp",
                table: "OccupancyLogs",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_OccupancyLogs_VehicleType",
                table: "OccupancyLogs",
                column: "VehicleType");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OccupancyLogs");
        }
    }
}
