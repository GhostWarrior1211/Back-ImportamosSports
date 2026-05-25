namespace ImportamosSports.Api.DTOs;

public class ValidarCuponRequest
{
    public string Codigo { get; set; } = string.Empty;
    public decimal Subtotal { get; set; }
}