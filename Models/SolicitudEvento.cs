using System.ComponentModel.DataAnnotations;

namespace ElectroGestion.Models;

public class SolicitudEvento
{
    public int Id { get; set; }
    public int SolicitudId { get; set; }
    public Solicitud? Solicitud { get; set; }
    public DateTime Fecha { get; set; } = DateTime.Now;

    [Required, StringLength(120)]
    public string Titulo { get; set; } = "";

    [StringLength(500)]
    public string Detalle { get; set; } = "";
}
