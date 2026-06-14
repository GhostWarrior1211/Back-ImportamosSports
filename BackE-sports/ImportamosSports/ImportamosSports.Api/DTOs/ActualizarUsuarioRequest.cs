namespace ImportamosSports.Api.DTOs;

public class ActualizarUsuarioRequest
{
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public int RolId { get; set; }
}