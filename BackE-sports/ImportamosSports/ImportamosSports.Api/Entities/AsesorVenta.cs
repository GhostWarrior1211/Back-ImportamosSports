namespace ImportamosSports.Api.Entities;

public class AsesorVenta
{
    public int Id { get; set; }
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string? FotoUrl { get; set; }
    public bool Activo { get; set; } = true;
}