namespace ImportamosSports.Api.Entities;

public class EstadoPedido
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;

    public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
}