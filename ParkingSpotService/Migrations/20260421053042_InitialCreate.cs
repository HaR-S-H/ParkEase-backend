using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ParkingSpotService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ParkingSpots",
                columns: table => new
                {
                    SpotId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LotId = table.Column<int>(type: "integer", nullable: false),
                    SpotNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Floor = table.Column<int>(type: "integer", nullable: false),
                    SpotType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    VehicleType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IsHandicapped = table.Column<bool>(type: "boolean", nullable: false),
                    IsEVCharging = table.Column<bool>(type: "boolean", nullable: false),
                    PricePerHour = table.Column<double>(type: "double precision", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParkingSpots", x => x.SpotId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ParkingSpots_IsEVCharging",
                table: "ParkingSpots",
                column: "IsEVCharging");

            migrationBuilder.CreateIndex(
                name: "IX_ParkingSpots_IsHandicapped",
                table: "ParkingSpots",
                column: "IsHandicapped");

            migrationBuilder.CreateIndex(
                name: "IX_ParkingSpots_LotId_SpotNumber",
                table: "ParkingSpots",
                columns: new[] { "LotId", "SpotNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ParkingSpots_LotId_SpotType",
                table: "ParkingSpots",
                columns: new[] { "LotId", "SpotType" });

            migrationBuilder.CreateIndex(
                name: "IX_ParkingSpots_LotId_Status",
                table: "ParkingSpots",
                columns: new[] { "LotId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ParkingSpots_LotId_VehicleType",
                table: "ParkingSpots",
                columns: new[] { "LotId", "VehicleType" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ParkingSpots");
        }
    }
}
