using System.ComponentModel.DataAnnotations;

namespace ElectroGestion.Models;

public class DetalleCotizacion
{
    public int Id { get; set; }
    public int CotizacionId { get; set; }
    public Cotizacion? Cotizacion { get; set; }

    // Snapshot of the inventory item at the time the quote is prepared.
    public int ArticuloInventarioId { get; set; }
    [StringLength(40)] public string CodigoArticulo { get; set; } = "";
    [StringLength(200)] public string NombreArticulo { get; set; } = "";
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal => Cantidad * PrecioUnitario;
}
