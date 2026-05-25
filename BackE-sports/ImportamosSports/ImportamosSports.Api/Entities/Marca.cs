using System.Text.Json.Serialization;

namespace ImportamosSports.Api.Entities;

public class Marca
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;

    [JsonIgnore]
    public ICollection<Producto> Productos { get; set; } = new List<Producto>();
}