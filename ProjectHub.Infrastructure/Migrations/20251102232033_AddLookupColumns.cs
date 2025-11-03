using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectHub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLookupColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LookupRelationshipId",
                table: "Columns",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LookupTargetColumnId",
                table: "Columns",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Columns_LookupRelationshipId",
                table: "Columns",
                column: "LookupRelationshipId");

            migrationBuilder.CreateIndex(
                name: "IX_Columns_LookupTargetColumnId",
                table: "Columns",
                column: "LookupTargetColumnId");

            migrationBuilder.AddForeignKey(
                name: "FK_Columns_Columns_LookupTargetColumnId",
                table: "Columns",
                column: "LookupTargetColumnId",
                principalTable: "Columns",
                principalColumn: "Column_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Columns_Relationships_LookupRelationshipId",
                table: "Columns",
                column: "LookupRelationshipId",
                principalTable: "Relationships",
                principalColumn: "RelationshipId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Columns_Columns_LookupTargetColumnId",
                table: "Columns");

            migrationBuilder.DropForeignKey(
                name: "FK_Columns_Relationships_LookupRelationshipId",
                table: "Columns");

            migrationBuilder.DropIndex(
                name: "IX_Columns_LookupRelationshipId",
                table: "Columns");

            migrationBuilder.DropIndex(
                name: "IX_Columns_LookupTargetColumnId",
                table: "Columns");

            migrationBuilder.DropColumn(
                name: "LookupRelationshipId",
                table: "Columns");

            migrationBuilder.DropColumn(
                name: "LookupTargetColumnId",
                table: "Columns");
        }
    }
}
