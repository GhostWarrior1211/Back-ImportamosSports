namespace ImportamosSports.Api.DTOs;

public class DetallePedidoRequest
{
    public int ProductoId { get; set; }
    public int Cantidad { get; set; }
    public decimal TallaUs { get; set; }
}