using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TurboRent.Modules.Catalog.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddListingModeration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "vehicle_listing_review_events",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ListingId = table.Column<Guid>(type: "uuid", nullable: false),
                    AdminUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Decision = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    ReasonCode = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vehicle_listing_review_events", x => x.Id);
                    table.CheckConstraint("ck_listing_review_decision", "\"Decision\" IN ('Approved','Rejected')");
                    table.CheckConstraint("ck_listing_review_reason", "(\"Decision\" = 'Approved' AND \"ReasonCode\" IS NULL) OR (\"Decision\" = 'Rejected' AND \"ReasonCode\" IN ('missing_information','invalid_vehicle_details','image_quality','other'))");
                    table.ForeignKey(
                        name: "FK_vehicle_listing_review_events_vehicle_listings_ListingId",
                        column: x => x.ListingId,
                        principalSchema: "catalog",
                        principalTable: "vehicle_listings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_listing_review_events_ListingId_CreatedAtUtc",
                schema: "catalog",
                table: "vehicle_listing_review_events",
                columns: new[] { "ListingId", "CreatedAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "vehicle_listing_review_events",
                schema: "catalog");
        }
    }
}
