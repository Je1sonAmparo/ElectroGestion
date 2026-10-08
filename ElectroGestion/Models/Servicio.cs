namespace ElectroGestion.Models;

public class Servicio
{
    public int Id { get; set; }
    public string ServicioId { get; set; } = "";
    public string Nombre { get; set; } = "";
    public string Categoria { get; set; } = "";
    public decimal PrecioBase { get; set; }
    public string Estado { get; set; } = "Activo";
}
