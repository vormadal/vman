using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VManBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddNoteToCollectionItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Note",
                table: "CollectionItems",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Note",
                table: "CollectionItems");
        }
    }
}
