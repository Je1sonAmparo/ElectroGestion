using System.ComponentModel.DataAnnotations;

namespace ElectroGestion.Models;

public class Levantamiento
{
    public int Id { get; set; }

    [Required]
    public int SolicitudId { get; set; }
    public Solicitud? Solicitud { get; set; }

    [StringLength(2000)]
    public string DisponibilidadEspacios { get; set; } = "";

    [StringLength(2000)]
    public string CargasElectricas { get; set; } = "";

    [StringLength(2000)]
    public string EstabilidadTension { get; set; } = "";

    [StringLength(2000)]
    public string RecorridoTuberias { get; set; } = "";

    [StringLength(2000)]
    public string RecorridoCableado { get; set; } = "";

    [StringLength(4000)]
    public string Observaciones { get; set; } = "";

    public string FotografiasJson { get; set; } = "[]";

    public bool BorradorGuardado { get; set; }
}
