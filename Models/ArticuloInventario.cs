namespace ElectroGestion.Models;

public class ArticuloInventario
{
    public int Id { get; set; }
    public string Codigo { get; set; } = "";
    public string Nombre { get; set; } = "";
    public string Categoria { get; set; } = "";
    public int Cantidad { get; set; }
    public decimal PrecioUnidad { get; set; }
}
