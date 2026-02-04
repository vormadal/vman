using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VManBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddItemsAndSyncJobs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ItemId",
                table: "ItemTags",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProviderName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ProviderItemId = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    OriginalFileName = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastSyncedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Items", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SyncJobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProviderName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    TotalItems = table.Column<int>(type: "integer", nullable: false),
                    ProcessedItems = table.Column<int>(type: "integer", nullable: false),
                    ErrorMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SyncJobs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ItemTags_ItemId",
                table: "ItemTags",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Items_CreatedAt",
                table: "Items",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Items_Provider",
                table: "Items",
                column: "ProviderName");

            migrationBuilder.CreateIndex(
                name: "IX_Items_Provider_ItemId",
                table: "Items",
                columns: new[] { "ProviderName", "ProviderItemId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SyncJobs_Provider_StartedAt",
                table: "SyncJobs",
                columns: new[] { "ProviderName", "StartedAt" });

            migrationBuilder.AddForeignKey(
                name: "FK_ItemTags_Items_ItemId",
                table: "ItemTags",
                column: "ItemId",
                principalTable: "Items",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItemTags_Items_ItemId",
                table: "ItemTags");

            migrationBuilder.DropTable(
                name: "Items");

            migrationBuilder.DropTable(
                name: "SyncJobs");

            migrationBuilder.DropIndex(
                name: "IX_ItemTags_ItemId",
                table: "ItemTags");

            migrationBuilder.DropColumn(
                name: "ItemId",
                table: "ItemTags");
        }
    }
}
