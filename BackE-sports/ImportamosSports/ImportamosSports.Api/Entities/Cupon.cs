namespace ImportamosSports.Api.Entities;

public class Cupon
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string TipoDescuento { get; set; } = string.Empty; // Porcentaje o MontoFijo
    public decimal Valor { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public decimal CompraMinima { get; set; }
}