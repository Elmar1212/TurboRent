using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TurboRent.Modules.Catalog.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOwnerListingsAndImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "OwnerUserId",
                schema: "catalog",
                table: "vehicle_listings",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "vehicle_listing_images",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ListingId = table.Column<Guid>(type: "uuid", nullable: false),
                    ObjectKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Length = table.Column<int>(type: "integer", nullable: false),
                    Position = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vehicle_listing_images", x => x.Id);
                    table.CheckConstraint("ck_listing_image_length", "\"Length\" BETWEEN 1 AND 5242880");
                    table.CheckConstraint("ck_listing_image_position", "\"Position\" BETWEEN 0 AND 4");
                    table.CheckConstraint("ck_listing_image_type", "\"ContentType\" IN ('image/jpeg','image/png','image/webp')");
                    table.ForeignKey(
                        name: "FK_vehicle_listing_images_vehicle_listings_ListingId",
                        column: x => x.ListingId,
                        principalSchema: "catalog",
                        principalTable: "vehicle_listings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_listings_OwnerUserId",
                schema: "catalog",
                table: "vehicle_listings",
                column: "OwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_listing_images_ListingId_Position",
                schema: "catalog",
                table: "vehicle_listing_images",
                columns: new[] { "ListingId", "Position" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_listing_images_ObjectKey",
                schema: "catalog",
                table: "vehicle_listing_images",
                column: "ObjectKey",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "vehicle_listing_images",
                schema: "catalog");

            migrationBuilder.DropIndex(
                name: "IX_vehicle_listings_OwnerUserId",
                schema: "catalog",
                table: "vehicle_listings");

            migrationBuilder.DropColumn(
                name: "OwnerUserId",
                schema: "catalog",
                table: "vehicle_listings");
        }
    }
}
