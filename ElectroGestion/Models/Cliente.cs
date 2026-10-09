using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElectroGestion.Models;

public class Cliente
{
    [Key]
    public int Id { get; set; }

    [Required, StringLength(150)]
    public string Nombre { get; set; } = "";

    [StringLength(150)]
    public string Correo { get; set; } = "";

    [StringLength(30)]
    public string Telefono { get; set; } = "";

    [StringLength(30)]
    public string Telefono2 { get; set; } = "";

    [StringLength(30)]
    public string Rnc { get; set; } = "";

    [StringLength(250)]
    public string Direccion { get; set; } = "";

    public int Servicios { get; set; }

    [StringLength(150)]
    public string Ultimo { get; set; } = "";

    public ICollection<ClienteDireccion> Direcciones { get; set; } = new List<ClienteDireccion>();
    public ICollection<Solicitud> Solicitudes { get; set; } = new List<Solicitud>();
}
