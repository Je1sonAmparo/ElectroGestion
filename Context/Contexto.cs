using Microsoft.EntityFrameworkCore;
using ElectroGestion.Models;

namespace ElectroGestion.Context;

public class Contexto : DbContext
{
    public Contexto(DbContextOptions<Contexto> options) : base(options) { }

    public DbSet<Solicitud> Solicitudes { get; set; }
    public DbSet<Visita> Visitas { get; set; }
    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Servicio> Servicios { get; set; }
    public DbSet<EstadoSolicitud> EstadosSolicitud { get; set; }
    public DbSet<DetalleSolicitud> DetallesSolicitud { get; set; }
    public DbSet<ClienteTelefono> ClientesTelefonos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasMany(c => c.Telefonos)
                .WithOne(t => t.Cliente)
                .HasForeignKey(t => t.ClienteId)
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

            entity.HasMany<Visita>()
                .WithOne(v => v.Solicitud)
                .HasForeignKey(v => v.SolicitudId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Visita>(entity =>
        {
            entity.HasOne(v => v.Tecnico)
                .WithMany()
                .HasForeignKey(v => v.TecnicoId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.Property(u => u.UsuarioLogin).HasMaxLength(50).IsRequired();
            entity.Property(u => u.PasswordHash).HasMaxLength(500).IsRequired();
            entity.Property(u => u.Nombre).HasMaxLength(150).IsRequired();
            entity.Property(u => u.Cargo).HasMaxLength(80).IsRequired();
            entity.Property(u => u.Rol).HasMaxLength(30).IsRequired();
            entity.HasIndex(u => u.UsuarioLogin).IsUnique();
        });

        modelBuilder.Entity<Usuario>().HasData(
            new Usuario
            {
                Id = 1,
                UsuarioLogin = "gerente",
                PasswordHash = "AQAAAAIAAYagAAAAEJvgEPmcnhCOwOXMhDWnexFdwrQh5Mp/b03A6SZkGM8aoTtqxVaYaiMdDG4D1BgbqQ==",
                Nombre = "Ana Hernández",
                Cargo = "Gerente",
                Rol = "Gerente"
            },
            new Usuario
            {
                Id = 2,
                UsuarioLogin = "tecnico",
                PasswordHash = "AQAAAAIAAYagAAAAEPeGfF62+i6tlvwb/sGQEx4ZyM4cP0sxOuSW3LIBCB8siO0FC0OAM/bXUzKQK/ujqw==",
                Nombre = "Luis Peña",
                Cargo = "Técnico",
                Rol = "Técnico"
            });

        modelBuilder.Entity<EstadoSolicitud>().HasData(
            new EstadoSolicitud { Id = 1, Nombre = "Pendiente" },
            new EstadoSolicitud { Id = 2, Nombre = "En proceso" },
            new EstadoSolicitud { Id = 3, Nombre = "Finalizada" },
            new EstadoSolicitud { Id = 4, Nombre = "Cancelada" }
        );

        modelBuilder.Entity<Cliente>().HasData(
            new Cliente { Id = 1, Nombre = "Melody Santos", Correo = "melody@email.com", Rnc = "402-1234567-1", Direccion = "Calle Duarte #24, Santiago", Servicios = 8, Ultimo = "Instalación de Sistema UPS" },
            new Cliente { Id = 2, Nombre = "Constructora López", Correo = "info@clopez.com", Rnc = "101-987654-2", Direccion = "Av. Estrella Sadhalá #102, Santiago", Servicios = 5, Ultimo = "Instalación Solar" },
            new Cliente { Id = 3, Nombre = "Farmacia El Sol", Correo = "farmacia@elsol.do", Rnc = "130-334455-3", Direccion = "Calle 16 #7, La Vega", Servicios = 6, Ultimo = "Mantenimiento Eléctrico" },
            new Cliente { Id = 4, Nombre = "Colmado Hernández", Correo = "hernandez@colmado.com", Rnc = "402-667788-4", Direccion = "Calle Principal #58, Moca", Servicios = 3, Ultimo = "Cableado Estructurado" },
            new Cliente { Id = 5, Nombre = "Taller Mecánico RD", Correo = "taller@mecanicord.com", Rnc = "130-112233-5", Direccion = "Carretera Luperón km 5, Santiago", Servicios = 1, Ultimo = "Instalación de Planta Eléctrica" }
        );

        modelBuilder.Entity<ClienteTelefono>().HasData(
            new ClienteTelefono { Id = 1, ClienteId = 1, Numero = "809-000-0000", Tipo = "Principal" },
            new ClienteTelefono { Id = 2, ClienteId = 1, Numero = "829-111-2222", Tipo = "Alternativo" },
            new ClienteTelefono { Id = 3, ClienteId = 2, Numero = "809-555-1212", Tipo = "Principal" },
            new ClienteTelefono { Id = 4, ClienteId = 3, Numero = "829-441-8890", Tipo = "Principal" },
            new ClienteTelefono { Id = 5, ClienteId = 4, Numero = "849-220-3311", Tipo = "Principal" },
            new ClienteTelefono { Id = 6, ClienteId = 5, Numero = "809-778-4402", Tipo = "Principal" },
            new ClienteTelefono { Id = 7, ClienteId = 5, Numero = "809-999-0000", Tipo = "Alternativo" }
        );

        modelBuilder.Entity<Servicio>().HasData(
            new Servicio { Id = 1, ServicioId = "SV-001", Nombre = "Instalación de Paneles Solares", Categoria = "Instalaciones", PrecioBase = 85000, Estado = "Activo" },
            new Servicio { Id = 2, ServicioId = "SV-002", Nombre = "Instalación de Sistema UPS", Categoria = "Instalaciones", PrecioBase = 45000, Estado = "Activo" },
            new Servicio { Id = 3, ServicioId = "SV-003", Nombre = "Mantenimiento Eléctrico", Categoria = "Mantenimiento", PrecioBase = 12500, Estado = "Activo" },
            new Servicio { Id = 4, ServicioId = "SV-004", Nombre = "Cableado Estructurado", Categoria = "Redes y Cableado", PrecioBase = 28000, Estado = "Activo" },
            new Servicio { Id = 5, ServicioId = "SV-005", Nombre = "Instalación de Planta Eléctrica", Categoria = "Instalaciones", PrecioBase = 120000, Estado = "Activo" }
        );

        modelBuilder.Entity<Solicitud>().HasData(
            new Solicitud
            {
                Id = 1,
                ClienteId = 1,
                ServicioId = 2,
                Contacto = "Melody Santos",
                Direccion = "Calle Duarte #24, Santiago",
                FechaSolicitud = new DateTime(2026, 10, 1),
                Fecha = new DateTime(2026, 10, 12),
                Hora = "09:00",
                EstadoSolicitudId = 1
            },
            new Solicitud
            {
                Id = 2,
                ClienteId = 2,
                ServicioId = 1,
                Contacto = "Constructora López",
                Direccion = "Av. Estrella Sadhalá #102, Santiago",
                FechaSolicitud = new DateTime(2026, 10, 2),
                Fecha = new DateTime(2026, 10, 10),
                Hora = "10:30",
                EstadoSolicitudId = 2
            },
            new Solicitud
            {
                Id = 3,
                ClienteId = 3,
                ServicioId = 3,
                Contacto = "Farmacia El Sol",
                Direccion = "Calle 16 #7, La Vega",
                FechaSolicitud = new DateTime(2026, 10, 3),
                Fecha = new DateTime(2026, 10, 11),
                Hora = "14:00",
                EstadoSolicitudId = 1
            },
            new Solicitud
            {
                Id = 4,
                ClienteId = 4,
                ServicioId = 4,
                Contacto = "Colmado Hernández",
                Direccion = "Calle Principal #58, Moca",
                FechaSolicitud = new DateTime(2026, 9, 29),
                Fecha = new DateTime(2026, 10, 5),
                Hora = "08:30",
                EstadoSolicitudId = 3
            },
            new Solicitud
            {
                Id = 5,
                ClienteId = 5,
                ServicioId = 5,
                Contacto = "Taller Mecánico RD",
                Direccion = "Carretera Luperón km 5, Santiago",
                FechaSolicitud = new DateTime(2026, 10, 4),
                Fecha = new DateTime(2026, 10, 9),
                Hora = "11:00",
                EstadoSolicitudId = 2
            },
            new Solicitud
            {
                Id = 6,
                ClienteId = 1,
                ServicioId = 3,
                Contacto = "Melody Santos",
                Direccion = "Calle Duarte #24, Santiago",
                FechaSolicitud = new DateTime(2026, 9, 20),
                Fecha = new DateTime(2026, 9, 28),
                Hora = "15:00",
                EstadoSolicitudId = 3
            },
            new Solicitud
            {
                Id = 7,
                ClienteId = 2,
                ServicioId = 4,
                Contacto = "Constructora López",
                Direccion = "Av. Estrella Sadhalá #102, Santiago",
                FechaSolicitud = new DateTime(2026, 9, 25),
                Fecha = new DateTime(2026, 10, 1),
                Hora = "09:30",
                EstadoSolicitudId = 4
            },
            new Solicitud
            {
                Id = 8,
                ClienteId = 3,
                ServicioId = 2,
                Contacto = "Farmacia El Sol",
                Direccion = "Calle 16 #7, La Vega",
                FechaSolicitud = new DateTime(2026, 10, 6),
                Fecha = new DateTime(2026, 10, 14),
                Hora = "10:00",
                EstadoSolicitudId = 1
            }
        );
    }
}
