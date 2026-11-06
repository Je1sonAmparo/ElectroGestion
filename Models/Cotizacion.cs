namespace ElectroGestion.Models;

public class Cotizacion
{
    public int Id { get; set; }
    public bool IsDeleted { get; set; }
    public string SolicitudId { get; set; } = "";
    public int? SolicitudDbId { get; set; }
    public Solicitud? Solicitud { get; set; }
    public string Cliente { get; set; } = "";
    public DateTime Fecha { get; set; }
    public decimal MontoTotal { get; set; }
    public decimal PorcentajeItbis { get; set; }
    public decimal MontoManoObra { get; set; }
    public string Estado { get; set; } = "En revisión";
    public DateTime? FechaEnvio { get; set; }
    public DateTime? FechaRespuesta { get; set; }
    public string MotivoRechazo { get; set; } = "";
    public ICollection<DetalleCotizacion> Detalles { get; set; } = new List<DetalleCotizacion>();
}
