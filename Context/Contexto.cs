using Microsoft.EntityFrameworkCore;
using ElectroGestion.Models;

namespace ElectroGestion.Context;

public class Contexto : DbContext
{
    public Contexto(DbContextOptions<Contexto> options) : base(options) { }

    public DbSet<Solicitud> Solicitudes { get; set; }
    public DbSet<SolicitudEvento> SolicitudesEventos { get; set; }
    public DbSet<Visita> Visitas { get; set; }
    public DbSet<Levantamiento> Levantamientos { get; set; }
    public DbSet<Cotizacion> Cotizaciones { get; set; }
    public DbSet<DetalleCotizacion> DetallesCotizacion { get; set; }
    public DbSet<OrdenTrabajo> OrdenesTrabajo { get; set; }
    public DbSet<Cobro> Cobros { get; set; }
    public DbSet<AbonoCobro> AbonosCobro { get; set; }
    public DbSet<ArticuloInventario> ArticulosInventario { get; set; }
    public DbSet<Venta> Ventas { get; set; }
    public DbSet<DetalleVenta> DetallesVenta { get; set; }
    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Servicio> Servicios { get; set; }
    public DbSet<EstadoSolicitud> EstadosSolicitud { get; set; }
    public DbSet<DetalleSolicitud> DetallesSolicitud { get; set; }
    public DbSet<ClienteTelefono> ClientesTelefonos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Venta>(entity =>
        {
            entity.HasQueryFilter(v => !v.IsDeleted);
            entity.Property(v => v.PrecioUnitario).HasPrecision(18, 2);
            entity.Property(v => v.TasaItbis).HasPrecision(5, 2);
            entity.Property(v => v.MontoRecibido).HasPrecision(18, 2);
            entity.HasIndex(v => v.Ncf).IsUnique();
            entity.HasIndex(v => new { v.Estado, v.FechaVenta });
            entity.HasOne<ArticuloInventario>().WithMany().HasForeignKey(v => v.ArticuloInventarioId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(v => v.Detalles).WithOne(d => d.Venta).HasForeignKey(d => d.VentaId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DetalleVenta>(entity =>
        {
            entity.HasQueryFilter(d => d.Venta != null && !d.Venta.IsDeleted);
            entity.Property(d => d.PrecioUnitario).HasPrecision(18, 2);
            entity.Ignore(d => d.Subtotal);
            entity.HasOne<ArticuloInventario>().WithMany().HasForeignKey(d => d.ArticuloInventarioId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ArticuloInventario>(entity =>
        {
            entity.HasQueryFilter(a => !a.IsDeleted);
            entity.Ignore(a => a.Estado);
            entity.Property(a => a.PrecioUnidad).HasPrecision(18, 2);
            entity.HasIndex(a => a.Codigo).IsUnique();
            entity.HasIndex(a => a.CodigoBarra).IsUnique().HasFilter("[CodigoBarra] IS NOT NULL");
            entity.HasData(
                new ArticuloInventario { Id = 1, Codigo = "ART-001", Nombre = "Cable de Cobre", Categoria = "Materiales", Cantidad = 500, PrecioUnidad = 15m },
                new ArticuloInventario { Id = 2, Codigo = "ART-002", Nombre = "Transformador 50kVA", Categoria = "Equipos", Cantidad = 10, PrecioUnidad = 25000m });
        });

        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasQueryFilter(c => !c.IsDeleted);
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
            entity.HasQueryFilter(s => !s.IsDeleted);
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

        modelBuilder.Entity<Levantamiento>(entity =>
        {
            entity.HasQueryFilter(l => l.Solicitud != null && !l.Solicitud.IsDeleted);
            entity.HasIndex(l => l.SolicitudId).IsUnique();
            entity.Property(l => l.FotografiasJson).HasColumnType("nvarchar(max)");
            entity.HasOne(l => l.Solicitud)
                .WithOne(s => s.Levantamiento)
                .HasForeignKey<Levantamiento>(l => l.SolicitudId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Cotizacion>(entity =>
        {
            entity.HasQueryFilter(c => !c.IsDeleted);
            entity.Property(c => c.Cliente).HasMaxLength(200).IsRequired();
            entity.Property(c => c.SolicitudId).HasMaxLength(50).IsRequired();
            entity.Property(c => c.Estado).HasMaxLength(40).IsRequired();
            entity.Property(c => c.MotivoRechazo).HasMaxLength(1000).IsRequired();
            entity.Property(c => c.MontoTotal).HasPrecision(18, 2);
            entity.Property(c => c.PorcentajeItbis).HasPrecision(5, 2);
            entity.Property(c => c.MontoManoObra).HasPrecision(18, 2);
            entity.HasOne(c => c.Solicitud)
                .WithMany()
                .HasForeignKey(c => c.SolicitudDbId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(c => c.SolicitudDbId).IsUnique().HasFilter("[SolicitudDbId] IS NOT NULL");
        });

        modelBuilder.Entity<DetalleCotizacion>(entity =>
        {
            entity.HasQueryFilter(d => d.Cotizacion != null && !d.Cotizacion.IsDeleted);
            entity.Property(d => d.CodigoArticulo).HasMaxLength(40).IsRequired();
            entity.Property(d => d.NombreArticulo).HasMaxLength(200).IsRequired();
            entity.Property(d => d.PrecioUnitario).HasPrecision(18, 2);
            entity.Ignore(d => d.Subtotal);
            entity.HasOne(d => d.Cotizacion)
                .WithMany(c => c.Detalles)
                .HasForeignKey(d => d.CotizacionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OrdenTrabajo>(entity =>
        {
            entity.HasQueryFilter(o => !o.IsDeleted);
            entity.Property(o => o.Code).HasMaxLength(20).IsRequired();
            entity.Property(o => o.Cliente).HasMaxLength(200).IsRequired();
            entity.Property(o => o.TipoServicio).HasMaxLength(200).IsRequired();
            entity.Property(o => o.Estado).HasMaxLength(30).IsRequired();
            entity.Property(o => o.TrabajosJson).HasColumnType("nvarchar(max)");
            entity.Property(o => o.MaterialesJson).HasColumnType("nvarchar(max)");
            entity.Property(o => o.HerramientasJson).HasColumnType("nvarchar(max)");
            entity.Property(o => o.Observaciones).HasMaxLength(2000);
            entity.HasIndex(o => o.CotizacionId).IsUnique();
            entity.HasOne(o => o.Cotizacion).WithMany().HasForeignKey(o => o.CotizacionId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<Solicitud>().WithMany().HasForeignKey(o => o.SolicitudId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Cobro>(entity =>
        {
            entity.HasQueryFilter(c => !c.IsDeleted);
            entity.Property(c => c.Cliente).HasMaxLength(200).IsRequired();
            entity.Property(c => c.MetodoPago).HasMaxLength(40).IsRequired();
            entity.Property(c => c.Estado).HasMaxLength(20).IsRequired();
            entity.Property(c => c.EstadoOrdenAntesEliminar).HasMaxLength(30);
            entity.Property(c => c.Monto).HasPrecision(18, 2);
            entity.HasIndex(c => c.CotizacionId).IsUnique();
            entity.HasIndex(c => c.OrdenTrabajoId);
            entity.HasOne(c => c.Cotizacion).WithMany().HasForeignKey(c => c.CotizacionId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(c => c.OrdenTrabajo).WithMany().HasForeignKey(c => c.OrdenTrabajoId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AbonoCobro>(entity =>
        {
            entity.HasQueryFilter(a => a.Cobro != null && !a.Cobro.IsDeleted);
            entity.Property(a => a.Monto).HasPrecision(18, 2);
            entity.Property(a => a.MetodoPago).HasMaxLength(40).IsRequired();
            entity.HasIndex(a => new { a.CobroId, a.Fecha });
            entity.HasOne(a => a.Cobro).WithMany(c => c.Abonos).HasForeignKey(a => a.CobroId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ClienteTelefono>()
            .HasQueryFilter(t => t.Cliente != null && !t.Cliente.IsDeleted);

        modelBuilder.Entity<SolicitudEvento>(entity =>
        {
            entity.HasQueryFilter(e => e.Solicitud != null && !e.Solicitud.IsDeleted);
            entity.Property(e => e.Titulo).HasMaxLength(120).IsRequired();
            entity.Property(e => e.Detalle).HasMaxLength(500).IsRequired();
            entity.HasIndex(e => new { e.SolicitudId, e.Fecha });
            entity.HasOne(e => e.Solicitud).WithMany(s => s.Eventos)
                .HasForeignKey(e => e.SolicitudId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<DetalleSolicitud>()
            .HasQueryFilter(d => d.Solicitud != null && !d.Solicitud.IsDeleted);

        modelBuilder.Entity<Solicitud>(entity =>
        {
            entity.HasQueryFilter(s => !s.IsDeleted);

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
            entity.HasQueryFilter(v => !v.IsDeleted && v.Solicitud != null && !v.Solicitud.IsDeleted);

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
                UsuarioLogin = "ana",
                PasswordHash = "AQAAAAIAAYagAAAAEHjAvVupymZcL4Z7wSkz9ro//N3Hyz31400GKF9GEPnxQFaOPv8TGpO2FJZ8XqCSwg==",
                Nombre = "Ana Hernández",
                Cargo = "Gerente",
                Rol = "Gerente"
            },
            new Usuario
            {
                Id = 2,
                UsuarioLogin = "luis",
                PasswordHash = "AQAAAAIAAYagAAAAEAXFkAX9eqMt9778g9tjjfmZCQWcmM5qlxzOI9VP6eXkDBo7XL52AGtJ9CTZWHUm7g==",
                Nombre = "Luis Peña",
                Cargo = "Técnico",
                Rol = "Técnico"
            },
            new Usuario
            {
                Id = 3,
                UsuarioLogin = "admin",
                PasswordHash = "AQAAAAIAAYagAAAAEDlXkB6f7Cn7W7LhwqonLz2YWMWnkh0z783OtK2aOGgJYxTR1gP7p26z3JILLccOWQ==",
                Nombre = "Administrador",
                Cargo = "Administrador",
                Rol = "Admin"
            });

        modelBuilder.Entity<EstadoSolicitud>().HasData(
            new EstadoSolicitud { Id = 1, Nombre = "Pendiente" },
            new EstadoSolicitud { Id = 2, Nombre = "En proceso" },
            new EstadoSolicitud { Id = 3, Nombre = "Finalizada" },
            new EstadoSolicitud { Id = 4, Nombre = "Cancelada" },
            new EstadoSolicitud { Id = 5, Nombre = "En Espera Administración" },
            new EstadoSolicitud { Id = 6, Nombre = "En revisión" },
            new EstadoSolicitud { Id = 7, Nombre = "Enviada al cliente" },
            new EstadoSolicitud { Id = 8, Nombre = "Aprobada" },
            new EstadoSolicitud { Id = 9, Nombre = "Rechazada" },
            new EstadoSolicitud { Id = 10, Nombre = "Pendiente de cobro" }
        );

        modelBuilder.Entity<Cliente>().HasData(
            new Cliente { Id = 1, Nombre = "Melody Santos", Correo = "melody@email.com", Rnc = "402-1234567-1", Direccion = "Calle Duarte #24, Santiago", Servicios = 8, Ultimo = "Instalación de Sistema UPS" },
            new Cliente { Id = 2, Nombre = "Constructora López", Correo = "info@clopez.com", Rnc = "101-98765-2", Direccion = "Av. Estrella Sadhalá #102, Santiago", Servicios = 5, Ultimo = "Instalación Solar" },
            new Cliente { Id = 3, Nombre = "Farmacia El Sol", Correo = "farmacia@elsol.do", Rnc = "130-33445-3", Direccion = "Calle 16 #7, La Vega", Servicios = 6, Ultimo = "Mantenimiento Eléctrico" },
            new Cliente { Id = 4, Nombre = "Colmado Hernández", Correo = "hernandez@colmado.com", Rnc = "402-6677884-1", Direccion = "Calle Principal #58, Moca", Servicios = 3, Ultimo = "Cableado Estructurado" },
            new Cliente { Id = 5, Nombre = "Taller Mecánico RD", Correo = "taller@mecanicord.com", Rnc = "130-11223-5", Direccion = "Carretera Luperón km 5, Santiago", Servicios = 1, Ultimo = "Instalación de Planta Eléctrica" }
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
            new Servicio { Id = 1, ServicioId = "SV-001", Nombre = "Instalación de Paneles Solares", Categoria = "Instalaciones", Estado = "Activo" },
            new Servicio { Id = 2, ServicioId = "SV-002", Nombre = "Instalación de Sistema UPS", Categoria = "Instalaciones", Estado = "Activo" },
            new Servicio { Id = 3, ServicioId = "SV-003", Nombre = "Mantenimiento Eléctrico", Categoria = "Mantenimiento", Estado = "Activo" },
            new Servicio { Id = 4, ServicioId = "SV-004", Nombre = "Cableado Estructurado", Categoria = "Redes y Cableado", Estado = "Activo" },
            new Servicio { Id = 5, ServicioId = "SV-005", Nombre = "Instalación de Planta Eléctrica", Categoria = "Instalaciones", Estado = "Activo" }
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
                EstadoSolicitudId = 1
            }
        );
    }
}
