using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ImportamosSports.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddDescuentoEnPedido : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CuponId",
                table: "Pedido",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DescuentoAplicado",
                table: "Pedido",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Igv",
                table: "Pedido",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Subtotal",
                table: "Pedido",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_Pedido_CuponId",
                table: "Pedido",
                column: "CuponId");

            migrationBuilder.AddForeignKey(
                name: "FK_Pedido_Cupon_CuponId",
                table: "Pedido",
                column: "CuponId",
                principalTable: "Cupon",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pedido_Cupon_CuponId",
                table: "Pedido");

            migrationBuilder.DropIndex(
                name: "IX_Pedido_CuponId",
                table: "Pedido");

            migrationBuilder.DropColumn(
                name: "CuponId",
                table: "Pedido");

            migrationBuilder.DropColumn(
                name: "DescuentoAplicado",
                table: "Pedido");

            migrationBuilder.DropColumn(
                name: "Igv",
                table: "Pedido");

            migrationBuilder.DropColumn(
                name: "Subtotal",
                table: "Pedido");
        }
    }
}
