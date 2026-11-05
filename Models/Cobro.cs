namespace ElectroGestion.Models;

public class Cobro
{
    public int Id { get; set; }
    public string Cliente { get; set; } = "";
    public int FacturaId { get; set; }
    public int CotizacionId { get; set; }
    public Cotizacion? Cotizacion { get; set; }
    public int? OrdenTrabajoId { get; set; }
    public OrdenTrabajo? OrdenTrabajo { get; set; }
    public DateTime Fecha { get; set; }
    public decimal Monto { get; set; }
    public string MetodoPago { get; set; } = "";
    public string Estado { get; set; } = "Pendiente de cobro";
    public ICollection<AbonoCobro> Abonos { get; set; } = new List<AbonoCobro>();

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public decimal MontoPagado => Abonos.Where(a => !a.Anulado).Sum(a => a.Monto);

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public decimal SaldoPendiente => Math.Max(0, Monto - MontoPagado);
}

