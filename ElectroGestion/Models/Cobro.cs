namespace ElectroGestion.Models;

public class Cobro
{
    public int Id { get; set; }
    public string Cliente { get; set; } = "";
    public int FacturaId { get; set; }
    public DateTime Fecha { get; set; }
    public decimal Monto { get; set; }
    public string MetodoPago { get; set; } = "";
    public string Estado { get; set; } = "Completado";
}
