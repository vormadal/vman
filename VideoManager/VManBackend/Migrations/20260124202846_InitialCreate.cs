using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VManBackend.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ImmichAssets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OriginalFileName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    OriginalPath = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    AssetType = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    FileCreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LocalDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Duration = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Width = table.Column<int>(type: "integer", nullable: true),
                    Height = table.Column<int>(type: "integer", nullable: true),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    IsFavorite = table.Column<bool>(type: "boolean", nullable: false),
                    IsArchived = table.Column<bool>(type: "boolean", nullable: false),
                    Rating = table.Column<double>(type: "double precision", nullable: true),
                    LastSyncedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImmichAssets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    LastLoginAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ImmichExifData",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    Make = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Model = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LensModel = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FNumber = table.Column<double>(type: "double precision", nullable: true),
                    FocalLength = table.Column<double>(type: "double precision", nullable: true),
                    Iso = table.Column<double>(type: "double precision", nullable: true),
                    ExposureTime = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Latitude = table.Column<double>(type: "double precision", nullable: true),
                    Longitude = table.Column<double>(type: "double precision", nullable: true),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    State = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImmichExifData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImmichExifData_ImmichAssets_AssetId",
                        column: x => x.AssetId,
                        principalTable: "ImmichAssets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SyncHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    TotalAssets = table.Column<int>(type: "integer", nullable: false),
                    SyncedAssets = table.Column<int>(type: "integer", nullable: false),
                    FailedAssets = table.Column<int>(type: "integer", nullable: false),
                    ErrorMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    AssetTypeFilter = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SyncHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SyncHistories_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ImmichAssets_AssetType",
                table: "ImmichAssets",
                column: "AssetType");

            migrationBuilder.CreateIndex(
                name: "IX_ImmichAssets_CreatedAt",
                table: "ImmichAssets",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ImmichAssets_LastSyncedAt",
                table: "ImmichAssets",
                column: "LastSyncedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ImmichExifData_AssetId",
                table: "ImmichExifData",
                column: "AssetId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SyncHistories_StartedAt",
                table: "SyncHistories",
                column: "StartedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SyncHistories_Status",
                table: "SyncHistories",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_SyncHistories_UserId",
                table: "SyncHistories",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ImmichExifData");

            migrationBuilder.DropTable(
                name: "SyncHistories");

            migrationBuilder.DropTable(
                name: "ImmichAssets");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
