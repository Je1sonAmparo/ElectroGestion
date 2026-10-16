using System.ComponentModel.DataAnnotations;

namespace ElectroGestion.Models;

public class Solicitud
{
    [Key]
    public int Id { get; set; }

    public bool IsDeleted { get; set; }

    public int ClienteId { get; set; }
    public Cliente? Cliente { get; set; }

    public int ServicioId { get; set; }
    public Servicio? Servicio { get; set; }

    [StringLength(50)]
    public string Contacto { get; set; } = "";

    [StringLength(250)]
    public string Direccion { get; set; } = "";

    public DateTime FechaSolicitud { get; set; }

    public int EstadoSolicitudId { get; set; }
    public EstadoSolicitud? EstadoSolicitud { get; set; }

    public DateTime? FechaFin { get; set; }

    [StringLength(150)]
    public string TecnicoAsignado { get; set; } = "";

    public ICollection<DetalleSolicitud> Detalles { get; set; } = new List<DetalleSolicitud>();
}
