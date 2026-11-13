using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TurboRent.Modules.Catalog.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class IncreaseVehicleImageSizeLimit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_listing_image_length",
                schema: "catalog",
                table: "vehicle_listing_images");
            migrationBuilder.AddCheckConstraint(
                name: "ck_listing_image_length",
                schema: "catalog",
                table: "vehicle_listing_images",
                sql: "\"Length\" BETWEEN 1 AND 52428800");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_listing_image_length",
                schema: "catalog",
                table: "vehicle_listing_images");
            migrationBuilder.AddCheckConstraint(
                name: "ck_listing_image_length",
                schema: "catalog",
                table: "vehicle_listing_images",
                sql: "\"Length\" BETWEEN 1 AND 5242880");
        }
    }
}
