using Microsoft.EntityFrameworkCore;
using ElectroGestion.Models;

namespace ElectroGestion.Context;

public class Contexto : DbContext
{
    public Contexto(DbContextOptions<Contexto> options) : base(options) { }

    public DbSet<Solicitud> Solicitudes { get; set; }
    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<Servicio> Servicios { get; set; }
    public DbSet<EstadoSolicitud> EstadosSolicitud { get; set; }
    public DbSet<DetalleSolicitud> DetallesSolicitud { get; set; }
    public DbSet<ClienteDireccion> ClientesDirecciones { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasMany(c => c.Direcciones)
                .WithOne(d => d.Cliente)
                .HasForeignKey(d => d.ClienteId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(c => c.Solicitudes)
                .WithOne(s => s.Cliente)
                .HasForeignKey(s => s.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Servicio>(entity =>
        {
            entity.HasMany(s => s.Solicitudes)
                .WithOne(sol => sol.Servicio)
                .HasForeignKey(sol => sol.ServicioId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(s => s.Detalles)
                .WithOne(d => d.Servicio)
                .HasForeignKey(d => d.ServicioId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<EstadoSolicitud>(entity =>
        {
            entity.HasMany(e => e.Solicitudes)
                .WithOne(s => s.EstadoSolicitud)
                .HasForeignKey(s => s.EstadoSolicitudId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Solicitud>(entity =>
        {
            entity.HasMany(s => s.Detalles)
                .WithOne(d => d.Solicitud)
                .HasForeignKey(d => d.SolicitudId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<EstadoSolicitud>().HasData(
            new EstadoSolicitud { Id = 1, Nombre = "Pendiente" },
            new EstadoSolicitud { Id = 2, Nombre = "En proceso" },
            new EstadoSolicitud { Id = 3, Nombre = "Finalizada" },
            new EstadoSolicitud { Id = 4, Nombre = "Cancelada" }
        );

        modelBuilder.Entity<Cliente>().HasData(
            new Cliente { Id = 1, Nombre = "Melody Santos", Correo = "melody@email.com", Telefono = "809-000-0000", Telefono2 = "829-111-2222", Rnc = "402-1234567-1", Direccion = "Calle Duarte #24, Santiago", Servicios = 8, Ultimo = "Instalación de Sistema UPS" },
            new Cliente { Id = 2, Nombre = "Constructora López", Correo = "info@clopez.com", Telefono = "809-555-1212", Telefono2 = "", Rnc = "101-987654-2", Direccion = "Av. Estrella Sadhalá #102, Santiago", Servicios = 5, Ultimo = "Instalación Solar" },
            new Cliente { Id = 3, Nombre = "Farmacia El Sol", Correo = "farmacia@elsol.do", Telefono = "829-441-8890", Telefono2 = "", Rnc = "130-334455-3", Direccion = "Calle 16 #7, La Vega", Servicios = 6, Ultimo = "Mantenimiento Eléctrico" },
            new Cliente { Id = 4, Nombre = "Colmado Hernández", Correo = "hernandez@colmado.com", Telefono = "849-220-3311", Telefono2 = "", Rnc = "402-667788-4", Direccion = "Calle Principal #58, Moca", Servicios = 3, Ultimo = "Cableado Estructurado" },
            new Cliente { Id = 5, Nombre = "Taller Mecánico RD", Correo = "taller@mecanicord.com", Telefono = "809-778-4402", Telefono2 = "809-999-0000", Rnc = "130-112233-5", Direccion = "Carretera Luperón km 5, Santiago", Servicios = 1, Ultimo = "Instalación de Planta Eléctrica" }
        );

        modelBuilder.Entity<ClienteDireccion>().HasData(
            new ClienteDireccion { Id = 1, ClienteId = 1, Direccion = "Calle Duarte #24, Santiago", Tipo = "Principal" },
            new ClienteDireccion { Id = 2, ClienteId = 2, Direccion = "Av. Estrella Sadhalá #102, Santiago", Tipo = "Principal" },
            new ClienteDireccion { Id = 3, ClienteId = 3, Direccion = "Calle 16 #7, La Vega", Tipo = "Principal" },
            new ClienteDireccion { Id = 4, ClienteId = 4, Direccion = "Calle Principal #58, Moca", Tipo = "Principal" },
            new ClienteDireccion { Id = 5, ClienteId = 5, Direccion = "Carretera Luperón km 5, Santiago", Tipo = "Principal" }
        );

        modelBuilder.Entity<Servicio>().HasData(
            new Servicio { Id = 1, ServicioId = "SV-001", Nombre = "Instalación de Paneles Solares", Categoria = "Instalaciones", PrecioBase = 85000, Estado = "Activo" },
            new Servicio { Id = 2, ServicioId = "SV-002", Nombre = "Instalación de Sistema UPS", Categoria = "Instalaciones", PrecioBase = 45000, Estado = "Activo" },
            new Servicio { Id = 3, ServicioId = "SV-003", Nombre = "Mantenimiento Eléctrico", Categoria = "Mantenimiento", PrecioBase = 12500, Estado = "Activo" },
            new Servicio { Id = 4, ServicioId = "SV-004", Nombre = "Cableado Estructurado", Categoria = "Redes y Cableado", PrecioBase = 28000, Estado = "Activo" },
            new Servicio { Id = 5, ServicioId = "SV-005", Nombre = "Instalación de Planta Eléctrica", Categoria = "Instalaciones", PrecioBase = 120000, Estado = "Activo" }
        );
    }
}

