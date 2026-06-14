using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ImportamosSports.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddProductoTallas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "TallaUs",
                table: "DetallePedido",
                type: "decimal(4,1)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "ProductoTalla",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductoId = table.Column<int>(type: "int", nullable: false),
                    TallaUs = table.Column<decimal>(type: "decimal(4,1)", nullable: false),
                    Stock = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductoTalla", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductoTalla_Producto_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Producto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductoTalla_ProductoId",
                table: "ProductoTalla",
                column: "ProductoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductoTalla");

            migrationBuilder.DropColumn(
                name: "TallaUs",
                table: "DetallePedido");
        }
    }
}
