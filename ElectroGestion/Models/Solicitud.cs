namespace ElectroGestion.Models;

public class Solicitud
{
    public int Id { get; set; }
    public string Cliente { get; set; } = "";
    public string Contacto { get; set; } = "";
    public string Direccion { get; set; } = "";
    public string TipoServicio { get; set; } = "";
    public string FechaSolicitud { get; set; } = "";
    public string Fecha { get; set; } = "";
    public string Hora { get; set; } = "";
    public string Estado { get; set; } = "Pendiente"; // Pendiente, En proceso, Finalizada, Cancelada
    public string? FechaFin { get; set; }
    public string TecnicoAsignado { get; set; } = "";
}
