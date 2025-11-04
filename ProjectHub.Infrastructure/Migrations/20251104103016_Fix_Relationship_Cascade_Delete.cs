using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectHub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Fix_Relationship_Cascade_Delete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Relationships_Columns_ForeignColumnId",
                table: "Relationships");

            migrationBuilder.DropForeignKey(
                name: "FK_Relationships_Tables_PrimaryTableId",
                table: "Relationships");

            migrationBuilder.AddForeignKey(
                name: "FK_Relationships_Columns_ForeignColumnId",
                table: "Relationships",
                column: "ForeignColumnId",
                principalTable: "Columns",
                principalColumn: "Column_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Relationships_Tables_PrimaryTableId",
                table: "Relationships",
                column: "PrimaryTableId",
                principalTable: "Tables",
                principalColumn: "Table_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Relationships_Columns_ForeignColumnId",
                table: "Relationships");

            migrationBuilder.DropForeignKey(
                name: "FK_Relationships_Tables_PrimaryTableId",
                table: "Relationships");

            migrationBuilder.AddForeignKey(
                name: "FK_Relationships_Columns_ForeignColumnId",
                table: "Relationships",
                column: "ForeignColumnId",
                principalTable: "Columns",
                principalColumn: "Column_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Relationships_Tables_PrimaryTableId",
                table: "Relationships",
                column: "PrimaryTableId",
                principalTable: "Tables",
                principalColumn: "Table_id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
