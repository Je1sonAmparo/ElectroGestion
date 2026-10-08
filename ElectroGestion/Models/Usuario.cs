namespace ElectroGestion.Models;

public class Usuario
{
    public int Id { get; set; }
    public string UsuarioLogin { get; set; } = "";
    public string Password { get; set; } = "";
    public string Nombre { get; set; } = "";
    public string Cargo { get; set; } = "";
    public string Rol { get; set; } = ""; // "Gerente" | "Técnico"

    public string Inicial => string.IsNullOrEmpty(Nombre) ? "?" : Nombre[..1].ToUpper();
}
