using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ProjectHub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRelationshipsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Relationships",
                columns: table => new
                {
                    RelationshipId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PrimaryTableId = table.Column<int>(type: "integer", nullable: false),
                    PrimaryColumnId = table.Column<int>(type: "integer", nullable: false),
                    ForeignTableId = table.Column<int>(type: "integer", nullable: false),
                    ForeignColumnId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Relationships", x => x.RelationshipId);
                    table.ForeignKey(
                        name: "FK_Relationships_Columns_ForeignColumnId",
                        column: x => x.ForeignColumnId,
                        principalTable: "Columns",
                        principalColumn: "Column_id");
                    table.ForeignKey(
                        name: "FK_Relationships_Columns_PrimaryColumnId",
                        column: x => x.PrimaryColumnId,
                        principalTable: "Columns",
                        principalColumn: "Column_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Relationships_Tables_ForeignTableId",
                        column: x => x.ForeignTableId,
                        principalTable: "Tables",
                        principalColumn: "Table_id");
                    table.ForeignKey(
                        name: "FK_Relationships_Tables_PrimaryTableId",
                        column: x => x.PrimaryTableId,
                        principalTable: "Tables",
                        principalColumn: "Table_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Relationships_ForeignColumnId",
                table: "Relationships",
                column: "ForeignColumnId");

            migrationBuilder.CreateIndex(
                name: "IX_Relationships_ForeignTableId",
                table: "Relationships",
                column: "ForeignTableId");

            migrationBuilder.CreateIndex(
                name: "IX_Relationships_PrimaryColumnId",
                table: "Relationships",
                column: "PrimaryColumnId");

            migrationBuilder.CreateIndex(
                name: "IX_Relationships_PrimaryTableId",
                table: "Relationships",
                column: "PrimaryTableId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Relationships");
        }
    }
}
