using System.ComponentModel.DataAnnotations;

namespace ElectroGestion.Models;

public class ClienteDireccion
{
    [Key]
    public int Id { get; set; }

    public int ClienteId { get; set; }
    public Cliente? Cliente { get; set; }

    [Required, StringLength(250)]
    public string Direccion { get; set; } = "";

    [StringLength(50)]
    public string Tipo { get; set; } = "Principal";
}
