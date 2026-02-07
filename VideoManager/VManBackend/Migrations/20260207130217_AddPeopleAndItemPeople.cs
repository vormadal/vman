using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VManBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddPeopleAndItemPeople : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "People",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProviderName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ProviderItemId = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    BirthDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ThumbnailPath = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsFavorite = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsHidden = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastSyncedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_People", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ItemPeople",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProviderName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ProviderItemId = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemPeople", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItemPeople_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ItemPeople_People_PersonId",
                        column: x => x.PersonId,
                        principalTable: "People",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ItemPeople_ItemId",
                table: "ItemPeople",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemPeople_PersonId",
                table: "ItemPeople",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemPeople_Provider_Item",
                table: "ItemPeople",
                columns: new[] { "ProviderName", "ProviderItemId" });

            migrationBuilder.CreateIndex(
                name: "IX_ItemPeople_Unique",
                table: "ItemPeople",
                columns: new[] { "PersonId", "ProviderName", "ProviderItemId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_People_Provider",
                table: "People",
                column: "ProviderName");

            migrationBuilder.CreateIndex(
                name: "IX_People_Provider_ItemId",
                table: "People",
                columns: new[] { "ProviderName", "ProviderItemId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ItemPeople");

            migrationBuilder.DropTable(
                name: "People");
        }
    }
}
