namespace ElectroGestion.Models;

public class Cobro
{
    public int Id { get; set; }
    public bool IsDeleted { get; set; }
    public string Cliente { get; set; } = "";
    [System.ComponentModel.DataAnnotations.MaxLength(30)]
    public string TipoComprobante { get; set; } = Venta.Consumo;
    [System.ComponentModel.DataAnnotations.MaxLength(30)]
    public string DocumentoCliente { get; set; } = "";
    [System.ComponentModel.DataAnnotations.MaxLength(13)]
    public string? Ncf { get; set; }
    public int FacturaId { get; set; }
    public int CotizacionId { get; set; }
    public Cotizacion? Cotizacion { get; set; }
    public int? OrdenTrabajoId { get; set; }
    public OrdenTrabajo? OrdenTrabajo { get; set; }
    public DateTime Fecha { get; set; }
    public decimal Monto { get; set; }
    public string MetodoPago { get; set; } = "";
    public string Estado { get; set; } = "Pendiente de cobro";
    [System.ComponentModel.DataAnnotations.MaxLength(30)]
    public string? EstadoOrdenAntesEliminar { get; set; }
    public int? EstadoSolicitudIdAntesEliminar { get; set; }
    public DateTime? FechaFinSolicitudAntesEliminar { get; set; }
    public ICollection<AbonoCobro> Abonos { get; set; } = new List<AbonoCobro>();

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public decimal MontoPagado => Abonos.Where(a => !a.Anulado).Sum(a => a.Monto);

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public decimal SaldoPendiente => Math.Max(0, Monto - MontoPagado);
}
