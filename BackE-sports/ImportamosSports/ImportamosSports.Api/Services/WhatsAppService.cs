using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ImportamosSports.Api.Services;

public class WhatsAppService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public WhatsAppService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<bool> EnviarCambioEstadoPedido(
        string telefono,
        string nombreCliente,
        int pedidoId,
        string estadoPedido,
        string? motivoCancelacion = null)
    {
        var pedidoTexto = pedidoId.ToString();
        var nombre = string.IsNullOrWhiteSpace(nombreCliente) ? "cliente" : nombreCliente.Trim();

        return estadoPedido switch
        {
            "Pagado" => await EnviarPlantilla(
                telefono,
                "pedido_pagado",
                "es",
                nombre,
                pedidoTexto
            ),

            "En preparación" => await EnviarPlantilla(
                telefono,
                "pedido_en_preparacion",
                "es",
                nombre,
                pedidoTexto
            ),

            "En camino" => await EnviarPlantilla(
                telefono,
                "pedido_en_camino",
                "es_PE",
                nombre,
                pedidoTexto,
                "Juan",
                "987654321"
            ),

            "Entregado" => await EnviarPlantilla(
                telefono,
                "pedido_entregado",
                "es",
                nombre,
                pedidoTexto
            ),

            "Cancelado" => await EnviarPlantilla(
                telefono,
                "pedido_cancelado",
                "es",
                nombre,
                pedidoTexto,
                string.IsNullOrWhiteSpace(motivoCancelacion) ? "Pedido cancelado" : motivoCancelacion
            ),

            _ => false
        };
    }

    public async Task<bool> EnviarPlantilla(
        string telefono,
        string nombrePlantilla,
        string codigoIdioma,
        params string[] parametros)
    {
        var accessToken = _configuration["WhatsApp:AccessToken"];
        var phoneNumberId = _configuration["WhatsApp:PhoneNumberId"];
        var apiVersion = _configuration["WhatsApp:ApiVersion"] ?? "v25.0";

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            Console.WriteLine("WHATSAPP: AccessToken vacío.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(phoneNumberId))
        {
            Console.WriteLine("WHATSAPP: PhoneNumberId vacío.");
            return false;
        }

        var telefonoLimpio = LimpiarTelefonoPeru(telefono);

        if (string.IsNullOrWhiteSpace(telefonoLimpio))
        {
            Console.WriteLine("WHATSAPP: Teléfono vacío o inválido.");
            return false;
        }

        var url = $"https://graph.facebook.com/{apiVersion}/{phoneNumberId}/messages";

        var body = new
        {
            messaging_product = "whatsapp",
            to = telefonoLimpio,
            type = "template",
            template = new
            {
                name = nombrePlantilla,
                language = new
                {
                    code = codigoIdioma
                },
                components = new object[]
                {
                    new
                    {
                        type = "body",
                        parameters = parametros.Select(p => new
                        {
                            type = "text",
                            text = p ?? ""
                        }).ToArray()
                    }
                }
            }
        };

        var json = JsonSerializer.Serialize(body);

        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request);
        var contenido = await response.Content.ReadAsStringAsync();

        Console.WriteLine($"RESPUESTA WHATSAPP {nombrePlantilla}:");
        Console.WriteLine($"Status: {(int)response.StatusCode} - {response.StatusCode}");
        Console.WriteLine(contenido);

        return response.IsSuccessStatusCode;
    }

    private string LimpiarTelefonoPeru(string telefono)
    {
        var limpio = new string((telefono ?? "").Where(char.IsDigit).ToArray());

        if (string.IsNullOrWhiteSpace(limpio))
            return "";

        if (limpio.StartsWith("51"))
            return limpio;

        return $"51{limpio}";
    }
}