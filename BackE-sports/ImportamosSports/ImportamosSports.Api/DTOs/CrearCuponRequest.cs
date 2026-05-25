namespace ImportamosSports.Api.DTOs;

public class CrearCuponRequest
{
    public string Codigo { get; set; } = string.Empty;
    public string TipoDescuento { get; set; } = string.Empty; // Porcentaje o MontoFijo
    public decimal Valor { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public decimal CompraMinima { get; set; }
}