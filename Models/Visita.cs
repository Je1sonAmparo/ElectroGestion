using System.ComponentModel.DataAnnotations;

namespace ElectroGestion.Models;

public class Visita
{
    [Key]
    public int Id { get; set; }

    public bool IsDeleted { get; set; }
    public bool EliminadaPorSolicitud { get; set; }

    public int SolicitudId { get; set; }
    public Solicitud? Solicitud { get; set; }

    public int? TecnicoId { get; set; }
    public Usuario? Tecnico { get; set; }

    public DateTime Fecha { get; set; }

    [Required, StringLength(10)]
    public string Hora { get; set; } = "";

    [Required, StringLength(10)]
    public string HoraFin { get; set; } = "";

    [Required, StringLength(30)]
    public string Estado { get; set; } = "Programada";
}
