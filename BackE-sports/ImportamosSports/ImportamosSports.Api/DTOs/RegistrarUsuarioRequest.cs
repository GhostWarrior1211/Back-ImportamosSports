namespace ImportamosSports.Api.DTOs;

public class RegistrarUsuarioRequest
{
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string Clave { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public int RolId { get; set; }
}