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
    public string Rnc { get; set; } = "";

    [StringLength(250)]
    public string Direccion { get; set; } = "";

    public int Servicios { get; set; }

    [StringLength(150)]
    public string Ultimo { get; set; } = "";

    public ICollection<ClienteTelefono> Telefonos { get; set; } = new List<ClienteTelefono>();
    public ICollection<Solicitud> Solicitudes { get; set; } = new List<Solicitud>();

    [NotMapped]
    public string Telefono
    {
        get => Telefonos.FirstOrDefault(t => t.Tipo == "Principal")?.Numero ?? Telefonos.FirstOrDefault()?.Numero ?? "";
        set
        {
            var p = Telefonos.FirstOrDefault(t => t.Tipo == "Principal");
            if (p != null) p.Numero = value;
            else if (!string.IsNullOrWhiteSpace(value)) Telefonos.Add(new ClienteTelefono { Numero = value, Tipo = "Principal" });
        }
    }

    [NotMapped]
    public string Telefono2
    {
        get => Telefonos.FirstOrDefault(t => t.Tipo == "Alternativo")?.Numero ?? Telefonos.Skip(1).FirstOrDefault()?.Numero ?? "";
        set
        {
            var a = Telefonos.FirstOrDefault(t => t.Tipo == "Alternativo");
            if (a != null) a.Numero = value;
            else if (!string.IsNullOrWhiteSpace(value)) Telefonos.Add(new ClienteTelefono { Numero = value, Tipo = "Alternativo" });
        }
    }
}
