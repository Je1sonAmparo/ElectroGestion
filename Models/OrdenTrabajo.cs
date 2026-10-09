namespace ElectroGestion.Models;

public class OrdenTrabajo
{
    public int Id { get; set; }
    public int SolicitudId { get; set; }
    public string Code { get; set; } = "";
    public string Cliente { get; set; } = "";
    public string TipoServicio { get; set; } = "";
    public string Estado { get; set; } = "En proceso"; // En proceso, Finalizada
    public List<string> Trabajos { get; set; } = new();
    public List<string> Materiales { get; set; } = new();
    public string Observaciones { get; set; } = "";
}
