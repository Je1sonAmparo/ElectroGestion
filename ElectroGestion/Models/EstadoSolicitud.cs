using System.ComponentModel.DataAnnotations;

namespace ElectroGestion.Models;

public class EstadoSolicitud
{
    [Key]
    public int Id { get; set; }

    [Required, StringLength(50)]
    public string Nombre { get; set; } = "";

    public ICollection<Solicitud> Solicitudes { get; set; } = new List<Solicitud>();
}
