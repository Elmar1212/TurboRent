using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TurboRent.Modules.Catalog.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "catalog");

            migrationBuilder.CreateTable(
                name: "vehicle_makes",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Slug = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    NameAz = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    NameEn = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vehicle_makes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "vehicle_models",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MakeId = table.Column<int>(type: "integer", nullable: false),
                    Slug = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    NameAz = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    NameEn = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vehicle_models", x => x.Id);
                    table.ForeignKey(
                        name: "FK_vehicle_models_vehicle_makes_MakeId",
                        column: x => x.MakeId,
                        principalSchema: "catalog",
                        principalTable: "vehicle_makes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "vehicle_listings",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ModelId = table.Column<int>(type: "integer", nullable: false),
                    Year = table.Column<short>(type: "smallint", nullable: false),
                    City = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DailyPrice = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    Transmission = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    FuelType = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Seats = table.Column<byte>(type: "smallint", nullable: false),
                    DescriptionAz = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    DescriptionEn = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vehicle_listings", x => x.Id);
                    table.CheckConstraint("ck_listing_city", "\"City\" IN ('baku','ganja','sumgait','lankaran','shaki','gabala')");
                    table.CheckConstraint("ck_listing_fuel", "\"FuelType\" IN ('petrol','diesel','hybrid','electric')");
                    table.CheckConstraint("ck_listing_price", "\"DailyPrice\" > 0 AND \"DailyPrice\" <= 99999");
                    table.CheckConstraint("ck_listing_seats", "\"Seats\" BETWEEN 2 AND 9");
                    table.CheckConstraint("ck_listing_status", "\"Status\" IN ('Draft','Pending','Approved','Rejected','Suspended')");
                    table.CheckConstraint("ck_listing_transmission", "\"Transmission\" IN ('automatic','manual')");
                    table.CheckConstraint("ck_listing_year", "\"Year\" BETWEEN 1980 AND 2035");
                    table.ForeignKey(
                        name: "FK_vehicle_listings_vehicle_models_ModelId",
                        column: x => x.ModelId,
                        principalSchema: "catalog",
                        principalTable: "vehicle_models",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                schema: "catalog",
                table: "vehicle_makes",
                columns: new[] { "Id", "NameAz", "NameEn", "Slug" },
                values: new object[,]
                {
                    { 1, "Toyota", "Toyota", "toyota" },
                    { 2, "Hyundai", "Hyundai", "hyundai" },
                    { 3, "Kia", "Kia", "kia" },
                    { 4, "Mercedes-Benz", "Mercedes-Benz", "mercedes-benz" },
                    { 5, "BMW", "BMW", "bmw" },
                    { 6, "Nissan", "Nissan", "nissan" }
                });

            migrationBuilder.InsertData(
                schema: "catalog",
                table: "vehicle_models",
                columns: new[] { "Id", "MakeId", "NameAz", "NameEn", "Slug" },
                values: new object[,]
                {
                    { 1, 1, "Camry", "Camry", "camry" },
                    { 2, 1, "Corolla", "Corolla", "corolla" },
                    { 3, 1, "Prius", "Prius", "prius" },
                    { 4, 2, "Elantra", "Elantra", "elantra" },
                    { 5, 2, "Sonata", "Sonata", "sonata" },
                    { 6, 2, "Tucson", "Tucson", "tucson" },
                    { 7, 3, "Sportage", "Sportage", "sportage" },
                    { 8, 3, "Optima", "Optima", "optima" },
                    { 9, 3, "Rio", "Rio", "rio" },
                    { 10, 4, "E-Class", "E-Class", "e-class" },
                    { 11, 4, "C-Class", "C-Class", "c-class" },
                    { 12, 4, "GLC", "GLC", "glc" },
                    { 13, 5, "3 Series", "3 Series", "3-series" },
                    { 14, 5, "5 Series", "5 Series", "5-series" },
                    { 15, 5, "X5", "X5", "x5" },
                    { 16, 6, "Altima", "Altima", "altima" },
                    { 17, 6, "X-Trail", "X-Trail", "x-trail" },
                    { 18, 6, "Qashqai", "Qashqai", "qashqai" }
                });

            migrationBuilder.Sql("""
                SELECT setval(pg_get_serial_sequence('catalog.vehicle_makes', 'Id'),
                    (SELECT MAX("Id") FROM catalog.vehicle_makes));
                SELECT setval(pg_get_serial_sequence('catalog.vehicle_models', 'Id'),
                    (SELECT MAX("Id") FROM catalog.vehicle_models));
                """);

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_listings_ModelId",
                schema: "catalog",
                table: "vehicle_listings",
                column: "ModelId");

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_listings_Status_City_DailyPrice",
                schema: "catalog",
                table: "vehicle_listings",
                columns: new[] { "Status", "City", "DailyPrice" });

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_listings_Status_ModelId_Year",
                schema: "catalog",
                table: "vehicle_listings",
                columns: new[] { "Status", "ModelId", "Year" });

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_makes_Slug",
                schema: "catalog",
                table: "vehicle_makes",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_models_MakeId_Slug",
                schema: "catalog",
                table: "vehicle_models",
                columns: new[] { "MakeId", "Slug" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "vehicle_listings",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "vehicle_models",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "vehicle_makes",
                schema: "catalog");
        }
    }
}
