using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ImportamosSports.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddComprobantePedido : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DireccionFiscalFactura",
                table: "Pedido",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NumeroComprobante",
                table: "Pedido",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "RazonSocialFactura",
                table: "Pedido",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RucFactura",
                table: "Pedido",
                type: "nvarchar(11)",
                maxLength: 11,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SerieComprobante",
                table: "Pedido",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TipoComprobante",
                table: "Pedido",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Boleta");

            migrationBuilder.InsertData(
                table: "EstadoPedido",
                columns: new[] { "Id", "Nombre" },
                values: new object[] { 6, "Cancelado" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "EstadoPedido",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DropColumn(
                name: "DireccionFiscalFactura",
                table: "Pedido");

            migrationBuilder.DropColumn(
                name: "NumeroComprobante",
                table: "Pedido");

            migrationBuilder.DropColumn(
                name: "RazonSocialFactura",
                table: "Pedido");

            migrationBuilder.DropColumn(
                name: "RucFactura",
                table: "Pedido");

            migrationBuilder.DropColumn(
                name: "SerieComprobante",
                table: "Pedido");

            migrationBuilder.DropColumn(
                name: "TipoComprobante",
                table: "Pedido");
        }
    }
}
