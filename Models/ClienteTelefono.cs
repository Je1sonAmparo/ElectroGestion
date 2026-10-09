using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElectroGestion.Models;

public class ClienteTelefono
{
    [Key]
    public int Id { get; set; }

    public int ClienteId { get; set; }
    [ForeignKey(nameof(ClienteId))]
    public Cliente? Cliente { get; set; }

    [Required, StringLength(30)]
    public string Numero { get; set; } = "";

    [StringLength(30)]
    public string Tipo { get; set; } = "Principal";
}
