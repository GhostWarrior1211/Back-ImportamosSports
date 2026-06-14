using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ImportamosSports.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddPagosPedidoUsuarios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AsesorVentaId",
                table: "Pedido",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EstadoPago",
                table: "Pedido",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Pendiente");

            migrationBuilder.AddColumn<string>(
                name: "MetodoPago",
                table: "Pedido",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Tarjeta");

            migrationBuilder.AddColumn<string>(
                name: "NumeroOperacion",
                table: "Pedido",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ObservacionPago",
                table: "Pedido",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pedido_AsesorVentaId",
                table: "Pedido",
                column: "AsesorVentaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Pedido_AsesorVenta_AsesorVentaId",
                table: "Pedido",
                column: "AsesorVentaId",
                principalTable: "AsesorVenta",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pedido_AsesorVenta_AsesorVentaId",
                table: "Pedido");

            migrationBuilder.DropIndex(
                name: "IX_Pedido_AsesorVentaId",
                table: "Pedido");

            migrationBuilder.DropColumn(
                name: "AsesorVentaId",
                table: "Pedido");

            migrationBuilder.DropColumn(
                name: "EstadoPago",
                table: "Pedido");

            migrationBuilder.DropColumn(
                name: "MetodoPago",
                table: "Pedido");

            migrationBuilder.DropColumn(
                name: "NumeroOperacion",
                table: "Pedido");

            migrationBuilder.DropColumn(
                name: "ObservacionPago",
                table: "Pedido");
        }
    }
}
