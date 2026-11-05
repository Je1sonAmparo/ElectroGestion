using System.ComponentModel.DataAnnotations;

namespace ElectroGestion.Models;

public class AbonoCobro
{
    public int Id { get; set; }
    public int CobroId { get; set; }
    public Cobro? Cobro { get; set; }
    public DateTime Fecha { get; set; } = DateTime.Now;
    public decimal Monto { get; set; }

    [Required, StringLength(40)]
    public string MetodoPago { get; set; } = "";

    public bool Anulado { get; set; }
}
