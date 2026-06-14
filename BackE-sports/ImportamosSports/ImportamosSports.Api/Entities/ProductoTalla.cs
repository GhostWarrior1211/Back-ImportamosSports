using System.Text.Json.Serialization;

namespace ImportamosSports.Api.Entities;

public class ProductoTalla
{
    public int Id { get; set; }

    public int ProductoId { get; set; }

    [JsonIgnore]
    public Producto? Producto { get; set; }

    public decimal TallaUs { get; set; }

    public int Stock { get; set; }
}