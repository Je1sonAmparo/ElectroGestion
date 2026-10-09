using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElectroGestion.Models;

public class DetalleSolicitud
{
    [Key]
    public int Id { get; set; }

    public int SolicitudId { get; set; }
    public Solicitud? Solicitud { get; set; }

    public int ServicioId { get; set; }
    public Servicio? Servicio { get; set; }

    [StringLength(250)]
    public string Descripcion { get; set; } = "";

    public int Cantidad { get; set; } = 1;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Precio { get; set; }
}
