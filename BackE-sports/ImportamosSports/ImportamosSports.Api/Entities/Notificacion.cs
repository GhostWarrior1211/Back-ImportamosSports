using System.Text.Json.Serialization;

namespace ImportamosSports.Api.Entities;

public class Notificacion
{
    public int Id { get; set; }
    public int PedidoId { get; set; }

    [JsonIgnore]
    public Pedido? Pedido { get; set; }

    public string Medio { get; set; } = "WhatsApp Simulado";
    public string Mensaje { get; set; } = string.Empty;
    public DateTime Fecha { get; set; } = DateTime.Now;
    public bool Enviado { get; set; } = true;
}