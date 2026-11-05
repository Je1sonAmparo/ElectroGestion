using System.ComponentModel.DataAnnotations;

namespace ElectroGestion.Models;

public class Venta
{
    public const string Consumo = "Consumidor final";
    public const string CreditoFiscal = "Crédito fiscal";
    public const string Borrador = "Borrador";
    public const string Registrada = "Registrada";
    public const string Anulada = "Anulada";

    public int Id { get; set; }
    public int ArticuloInventarioId { get; set; }
    public int? ClienteId { get; set; }
    [MaxLength(40)] public string CodigoArticulo { get; set; } = "";
    [MaxLength(200)] public string NombreArticulo { get; set; } = "";
    [MaxLength(200)] public string NombreCliente { get; set; } = "";
    [MaxLength(30)] public string DocumentoCliente { get; set; } = "";
    [MaxLength(30)] public string TipoComprobante { get; set; } = Consumo;
    [MaxLength(13)] public string? Ncf { get; set; }
    [MaxLength(20)] public string Estado { get; set; } = Borrador;
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaVenta { get; set; }
    public int Cantidad { get; set; } = 1;
    public decimal PrecioUnitario { get; set; }
    public decimal TasaItbis { get; set; } = 18m;
    public decimal Subtotal => Cantidad * PrecioUnitario;
    public decimal Itbis => Math.Round(Subtotal * TasaItbis / 100m, 2, MidpointRounding.AwayFromZero);
    public decimal Total => Subtotal + Itbis;
}
