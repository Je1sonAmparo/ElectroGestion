using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace ElectroGestion.Models;

public class OrdenTrabajo
{
    public int Id { get; set; }
    public bool IsDeleted { get; set; }
    public int SolicitudId { get; set; }
    public int CotizacionId { get; set; }
    public Cotizacion? Cotizacion { get; set; }
    public string Code { get; set; } = "";
    public string Cliente { get; set; } = "";
    public string TipoServicio { get; set; } = "";
    public string Estado { get; set; } = "En proceso";
    public DateTime FechaCreacion { get; set; } = DateTime.Now;
    public string TrabajosJson { get; set; } = "[]";
    public string MaterialesJson { get; set; } = "[]";
    public string HerramientasJson { get; set; } = "[]";
    public string Observaciones { get; set; } = "";

    [NotMapped]
    public List<string> Trabajos
    {
        get => LeerLista(TrabajosJson);
        set => TrabajosJson = JsonSerializer.Serialize(value);
    }

    [NotMapped]
    public List<MaterialOrden> MaterialesCotizados
    {
        get => JsonSerializer.Deserialize<List<MaterialOrden>>(MaterialesJson) ?? [];
        set => MaterialesJson = JsonSerializer.Serialize(value);
    }

    [NotMapped]
    public List<string> Herramientas
    {
        get => LeerLista(HerramientasJson);
        set => HerramientasJson = JsonSerializer.Serialize(value);
    }

    private static List<string> LeerLista(string json) => JsonSerializer.Deserialize<List<string>>(json) ?? [];
}

public class MaterialOrden
{
    public string Codigo { get; set; } = "";
    public string Nombre { get; set; } = "";
    public int Cantidad { get; set; }
}
