using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ImportamosSports.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddJwtAndSeedAdmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Usuario",
                columns: new[] { "Id", "Activo", "Apellidos", "Clave", "Correo", "Nombres", "RolId", "Telefono" },
                values: new object[] { 1, true, "Sistema", "123456", "admin@importamossports.com", "Admin", 1, "999999999" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "Id",
                keyValue: 1);
        }
    }
}
