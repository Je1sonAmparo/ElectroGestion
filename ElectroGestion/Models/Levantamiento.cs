namespace ElectroGestion.Models;

public class Levantamiento
{
    public int Id { get; set; }
    public int SolicitudId { get; set; }
    public string Estado { get; set; } = "Pendiente"; // Pendiente, En Espera Administración
    public Dictionary<string, string> Datos { get; set; } = new();
    public bool Borrador { get; set; }
}
