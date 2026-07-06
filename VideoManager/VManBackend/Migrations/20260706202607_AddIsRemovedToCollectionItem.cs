using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VManBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddIsRemovedToCollectionItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsRemoved",
                table: "CollectionItems",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "RemovedAt",
                table: "CollectionItems",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsRemoved",
                table: "CollectionItems");

            migrationBuilder.DropColumn(
                name: "RemovedAt",
                table: "CollectionItems");
        }
    }
}
