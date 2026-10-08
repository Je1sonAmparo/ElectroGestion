namespace ElectroGestion.Models;

public class Cliente
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string Correo { get; set; } = "";
    public string Telefono { get; set; } = "";
    public string Telefono2 { get; set; } = "";
    public string Rnc { get; set; } = "";
    public string Direccion { get; set; } = "";
    public int Servicios { get; set; }
    public string Ultimo { get; set; } = "";
}
