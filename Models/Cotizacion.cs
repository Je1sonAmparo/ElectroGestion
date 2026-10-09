namespace ElectroGestion.Models;

public class Cotizacion
{
    public int Id { get; set; }
    public string SolicitudId { get; set; } = "";
    public string Cliente { get; set; } = "";
    public DateTime Fecha { get; set; }
    public decimal MontoTotal { get; set; }
    public string Estado { get; set; } = "Pendiente";
}
