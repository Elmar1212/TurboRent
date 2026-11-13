using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TurboRent.Modules.Catalog.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBookingsAndOwnerManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_listing_status",
                schema: "catalog",
                table: "vehicle_listings");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAtUtc",
                schema: "catalog",
                table: "vehicle_listings",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "bookings",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ListingId = table.Column<Guid>(type: "uuid", nullable: false),
                    RenterUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnerUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    DailyPrice = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    TotalPrice = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Message = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DecidedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Version = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bookings", x => x.Id);
                    table.CheckConstraint("ck_booking_dates", "\"EndDate\" >= \"StartDate\" AND \"EndDate\" - \"StartDate\" < 60");
                    table.CheckConstraint("ck_booking_decision", "(\"Status\" = 'Requested' AND \"DecidedAtUtc\" IS NULL) OR (\"Status\" <> 'Requested' AND \"DecidedAtUtc\" IS NOT NULL)");
                    table.CheckConstraint("ck_booking_parties", "\"RenterUserId\" <> \"OwnerUserId\"");
                    table.CheckConstraint("ck_booking_price", "\"DailyPrice\" > 0 AND \"TotalPrice\" > 0");
                    table.CheckConstraint("ck_booking_status", "\"Status\" IN ('Requested','Confirmed','Declined','Cancelled')");
                    table.ForeignKey(
                        name: "FK_bookings_vehicle_listings_ListingId",
                        column: x => x.ListingId,
                        principalSchema: "catalog",
                        principalTable: "vehicle_listings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_listings_Status_CreatedAtUtc",
                schema: "catalog",
                table: "vehicle_listings",
                columns: new[] { "Status", "CreatedAtUtc" });

            migrationBuilder.AddCheckConstraint(
                name: "ck_listing_status",
                schema: "catalog",
                table: "vehicle_listings",
                sql: "\"Status\" IN ('Draft','Pending','Approved','Rejected','Suspended','Archived')");

            migrationBuilder.CreateIndex(
                name: "IX_bookings_ListingId_Status_StartDate",
                schema: "catalog",
                table: "bookings",
                columns: new[] { "ListingId", "Status", "StartDate" });

            migrationBuilder.CreateIndex(
                name: "IX_bookings_OwnerUserId_Status_StartDate",
                schema: "catalog",
                table: "bookings",
                columns: new[] { "OwnerUserId", "Status", "StartDate" });

            migrationBuilder.CreateIndex(
                name: "IX_bookings_RenterUserId_CreatedAtUtc",
                schema: "catalog",
                table: "bookings",
                columns: new[] { "RenterUserId", "CreatedAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bookings",
                schema: "catalog");

            migrationBuilder.DropIndex(
                name: "IX_vehicle_listings_Status_CreatedAtUtc",
                schema: "catalog",
                table: "vehicle_listings");

            migrationBuilder.DropCheckConstraint(
                name: "ck_listing_status",
                schema: "catalog",
                table: "vehicle_listings");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                schema: "catalog",
                table: "vehicle_listings");

            migrationBuilder.AddCheckConstraint(
                name: "ck_listing_status",
                schema: "catalog",
                table: "vehicle_listings",
                sql: "\"Status\" IN ('Draft','Pending','Approved','Rejected','Suspended')");
        }
    }
}
