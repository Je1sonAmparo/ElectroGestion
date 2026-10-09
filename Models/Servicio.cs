using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElectroGestion.Models;

public class Servicio
{
    [Key]
    public int Id { get; set; }

    [Required, StringLength(20)]
    public string ServicioId { get; set; } = "";

    [Required, StringLength(150)]
    public string Nombre { get; set; } = "";

    [StringLength(80)]
    public string Categoria { get; set; } = "";

    [Column(TypeName = "decimal(18,2)")]
    public decimal PrecioBase { get; set; }

    [StringLength(30)]
    public string Estado { get; set; } = "Activo";

    public ICollection<Solicitud> Solicitudes { get; set; } = new List<Solicitud>();
    public ICollection<DetalleSolicitud> Detalles { get; set; } = new List<DetalleSolicitud>();
}
