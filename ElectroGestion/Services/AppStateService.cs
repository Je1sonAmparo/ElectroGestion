using ElectroGestion.Models;

namespace ElectroGestion.Services;

public class AppStateService
{
    public Usuario? Session { get; set; }
    public bool IsAuthenticated => Session != null;
    public bool EsGerente => Session?.Rol == "Gerente";

    public List<Usuario> Usuarios { get; set; } = new();
    public List<Cliente> Clientes { get; set; } = new();
    public List<Solicitud> Solicitudes { get; set; } = new();
    public List<Levantamiento> Levantamientos { get; set; } = new();
    public List<OrdenTrabajo> Ordenes { get; set; } = new();
    public List<Servicio> Servicios { get; set; } = new();
    public List<EstadoSolicitud> EstadosSolicitud { get; set; } = new();
    public List<DetalleSolicitud> DetallesSolicitud { get; set; } = new();
    public List<Cotizacion> Cotizaciones { get; set; } = new();
    public List<Cobro> Cobros { get; set; } = new();
    public List<ArticuloInventario> Inventario { get; set; } = new();
    public List<Proveedor> Proveedores { get; set; } = new();


    public event Action? OnChange;
    public void NotifyStateChanged()
    {
        if (OnChange == null) return;
        foreach (Action handler in OnChange.GetInvocationList())
        {
            try
            {
                handler();
            }
            catch
            {
                // Prevent thread dispatch issues from crashing circuit
            }
        }
    }

    private static string Iso(DateTime? d) => d.HasValue ? d.Value.ToString("yyyy-MM-dd") : "";
    private static DateTime DateOffset(int days) => DateTime.Today.AddDays(days);

    public AppStateService()
    {
        SeedData();
    }

    public void Login(Usuario user)
    {
        Session = user;
        NotifyStateChanged();
    }

    public void Logout()
    {
        Session = null;
        NotifyStateChanged();
    }

    private void SeedData()
    {
        Usuarios = new List<Usuario>
        {
            new() { Id = 1, UsuarioLogin = "gerente", Password = "gerente", Nombre = "Ana Hernández", Cargo = "Gerente", Rol = "Gerente" },
            new() { Id = 2, UsuarioLogin = "tecnico", Password = "tecnico", Nombre = "Luis Peña", Cargo = "Técnico", Rol = "Técnico" }
        };

        Clientes = new List<Cliente>
        {
            new() { Id = 1, Nombre = "Melody Santos", Correo = "melody@email.com", Telefono = "809-000-0000", Telefono2 = "829-111-2222", Rnc = "402-1234567-1", Direccion = "Calle Duarte #24, Santiago", Servicios = 8, Ultimo = "Instalación de Sistema UPS" },
            new() { Id = 2, Nombre = "Constructora López", Correo = "info@clopez.com", Telefono = "809-555-1212", Telefono2 = "", Rnc = "101-987654-2", Direccion = "Av. Estrella Sadhalá #102, Santiago", Servicios = 5, Ultimo = "Instalación Solar" },
            new() { Id = 3, Nombre = "Farmacia El Sol", Correo = "farmacia@elsol.do", Telefono = "829-441-8890", Telefono2 = "", Rnc = "130-334455-3", Direccion = "Calle 16 #7, La Vega", Servicios = 6, Ultimo = "Mantenimiento Eléctrico" },
            new() { Id = 4, Nombre = "Colmado Hernández", Correo = "hernandez@colmado.com", Telefono = "849-220-3311", Telefono2 = "", Rnc = "402-667788-4", Direccion = "Calle Principal #58, Moca", Servicios = 3, Ultimo = "Cableado Estructurado" },
            new() { Id = 5, Nombre = "Taller Mecánico RD", Correo = "taller@mecanicord.com", Telefono = "809-778-4402", Telefono2 = "809-999-0000", Rnc = "130-112233-5", Direccion = "Carretera Luperón km 5, Santiago", Servicios = 1, Ultimo = "Instalación de Planta Eléctrica" }
        };

        EstadosSolicitud = new List<EstadoSolicitud>
        {
            new() { Id = 1, Nombre = "Pendiente" },
            new() { Id = 2, Nombre = "En proceso" },
            new() { Id = 3, Nombre = "Finalizada" },
            new() { Id = 4, Nombre = "Cancelada" }
        };

        Solicitudes = new List<Solicitud>
        {
            new() { Id = 1, ClienteId = 1, Cliente = Clientes[0], Contacto = "809-000-0000", Direccion = "Calle Duarte #24, Santiago", ServicioId = 2, Servicio = Servicios[1], FechaSolicitud = DateTime.Today.AddDays(-6), Fecha = DateTime.Today, Hora = "09:00", EstadoSolicitudId = 2, EstadoSolicitud = EstadosSolicitud[1] },
            new() { Id = 2, ClienteId = 2, Cliente = Clientes[1], Contacto = "809-555-1212", Direccion = "Av. Estrella Sadhalá #102, Santiago", ServicioId = 1, Servicio = Servicios[0], FechaSolicitud = DateTime.Today.AddDays(-5), Fecha = DateTime.Today, Hora = "14:00", EstadoSolicitudId = 1, EstadoSolicitud = EstadosSolicitud[0] },
            new() { Id = 3, ClienteId = 3, Cliente = Clientes[2], Contacto = "829-441-8890", Direccion = "Calle 16 #7, La Vega", ServicioId = 3, Servicio = Servicios[2], FechaSolicitud = DateTime.Today.AddDays(-4), Fecha = DateTime.Today.AddDays(1), Hora = "10:30", EstadoSolicitudId = 1, EstadoSolicitud = EstadosSolicitud[0] },
            new() { Id = 4, ClienteId = 4, Cliente = Clientes[3], Contacto = "849-220-3311", Direccion = "Calle Principal #58, Moca", ServicioId = 4, Servicio = Servicios[3], FechaSolicitud = DateTime.Today.AddDays(-3), Fecha = DateTime.Today.AddDays(2), Hora = "08:00", EstadoSolicitudId = 1, EstadoSolicitud = EstadosSolicitud[0] },
            new() { Id = 5, ClienteId = 5, Cliente = Clientes[4], Contacto = "809-778-4402", Direccion = "Carretera Luperón km 5, Santiago", ServicioId = 5, Servicio = Servicios[4], FechaSolicitud = DateTime.Today.AddDays(-2), Fecha = DateTime.Today, Hora = "", EstadoSolicitudId = 1, EstadoSolicitud = EstadosSolicitud[0] },
            new() { Id = 6, ClienteId = 1, Cliente = Clientes[0], Contacto = "809-000-0000", Direccion = "Calle Duarte #24, Santiago", ServicioId = 3, Servicio = Servicios[2], FechaSolicitud = DateTime.Today.AddDays(-12), Fecha = DateTime.Today.AddDays(-9), Hora = "11:00", EstadoSolicitudId = 3, EstadoSolicitud = EstadosSolicitud[2], FechaFin = DateTime.Today.AddDays(-8) },
            new() { Id = 7, ClienteId = 3, Cliente = Clientes[2], Contacto = "829-441-8890", Direccion = "Calle 16 #7, La Vega", ServicioId = 4, Servicio = Servicios[3], FechaSolicitud = DateTime.Today.AddDays(-20), Fecha = DateTime.Today.AddDays(-18), Hora = "09:00", EstadoSolicitudId = 3, EstadoSolicitud = EstadosSolicitud[2], FechaFin = DateTime.Today.AddDays(-17) },
            new() { Id = 8, ClienteId = 4, Cliente = Clientes[3], Contacto = "849-220-3311", Direccion = "Calle Principal #58, Moca", ServicioId = 1, Servicio = Servicios[0], FechaSolicitud = DateTime.Today.AddDays(-30), Fecha = DateTime.Today.AddDays(-28), Hora = "15:00", EstadoSolicitudId = 4, EstadoSolicitud = EstadosSolicitud[3] }
        };

        Levantamientos = new List<Levantamiento>
        {
            new() { Id = 1, SolicitudId = 2, Estado = "Pendiente" },
            new() { Id = 2, SolicitudId = 3, Estado = "Pendiente" },
            new() { Id = 3, SolicitudId = 4, Estado = "Pendiente" },
            new() { Id = 4, SolicitudId = 1, Estado = "En Espera Administración" }
        };

        Ordenes = new List<OrdenTrabajo>
        {
            new()
            {
                Id = 1, Code = "OT-0001", SolicitudId = 1, Estado = "En proceso",
                Trabajos = new() { "Montaje del gabinete UPS en sala técnica", "Conexión de banco de baterías y puesta a tierra", "Pruebas de transferencia y respaldo (30 min)" },
                Materiales = new() { "UPS 6 kVA", "Baterías 12V x8", "Cable THHN #6", "Breaker 60A", "Canaleta metálica", "Multímetro" },
                Observaciones = "Coordinar con el cliente el corte de energía entre 9:00 y 11:00 AM. Requiere dos técnicos."
            },
            new()
            {
                Id = 2, Code = "OT-0002", SolicitudId = 2, Estado = "En proceso",
                Trabajos = new() { "Instalación de estructura en techo (12 paneles)", "Tendido de tubería y cableado DC", "Configuración de inversor y monitoreo" },
                Materiales = new() { "Paneles 550W x12", "Inversor híbrido 6kW", "Riel de aluminio", "Tubería EMT 3/4", "Cable solar #10", "Herramienta de crimpado" },
                Observaciones = "Verificar disponibilidad de andamio en obra. Entregar informe fotográfico al finalizar."
            }
        };

        Servicios = new List<Servicio>
        {
            new() { Id = 1, ServicioId = "SV-001", Nombre = "Instalación de Paneles Solares", Categoria = "Instalaciones", PrecioBase = 85000, Estado = "Activo" },
            new() { Id = 2, ServicioId = "SV-002", Nombre = "Instalación de Sistema UPS", Categoria = "Instalaciones", PrecioBase = 45000, Estado = "Activo" },
            new() { Id = 3, ServicioId = "SV-003", Nombre = "Mantenimiento Eléctrico", Categoria = "Mantenimiento", PrecioBase = 12500, Estado = "Activo" },
            new() { Id = 4, ServicioId = "SV-004", Nombre = "Cableado Estructurado", Categoria = "Redes y Cableado", PrecioBase = 28000, Estado = "Activo" },
            new() { Id = 5, ServicioId = "SV-005", Nombre = "Instalación de Planta Eléctrica", Categoria = "Instalaciones", PrecioBase = 120000, Estado = "Activo" }
        };
        Cotizaciones = new List<Cotizacion>
        {
            new() { Id = 1, SolicitudId = "REQ-001", Cliente = "Acme Corp", Fecha = DateTime.Today.AddDays(-2), MontoTotal = 15000.00m, Estado = "Aprobada" },
            new() { Id = 2, SolicitudId = "REQ-002", Cliente = "Globex S.A.", Fecha = DateTime.Today.AddDays(-1), MontoTotal = 8500.50m, Estado = "Pendiente" }
        };

        Cobros = new List<Cobro>
        {
            new() { Id = 1, Cliente = "Acme Corp", FacturaId = 1001, Fecha = DateTime.Today.AddDays(-1), Monto = 7500.00m, MetodoPago = "Transferencia", Estado = "Completado" },
            new() { Id = 2, Cliente = "Globex S.A.", FacturaId = 1002, Fecha = DateTime.Today, Monto = 4000.00m, MetodoPago = "Cheque", Estado = "Pendiente" }
        };

        Inventario = new List<ArticuloInventario>
        {
            new() { Id = 1, Codigo = "ART-001", Nombre = "Cable de Cobre", Categoria = "Materiales", Cantidad = 500, PrecioUnidad = 15.00m },
            new() { Id = 2, Codigo = "ART-002", Nombre = "Transformador 50kVA", Categoria = "Equipos", Cantidad = 10, PrecioUnidad = 25000.00m }
        };

        Proveedores = new List<Proveedor>
        {
            new() { Id = 1, Nombre = "Ferretería Industrial", Rnc = "101-23456-7", Correo = "ventas@ferreind.com", Telefono = "809-555-1234", Direccion = "Av. Principal #123" },
            new() { Id = 2, Nombre = "Equipos Eléctricos S.R.L.", Rnc = "130-98765-4", Correo = "contacto@equiposelec.com", Telefono = "829-555-9876", Direccion = "Zona Industrial Haina" }
        };
    }

    public int NextSolicitudId() => Solicitudes.Count > 0 ? Solicitudes.Max(s => s.Id) + 1 : 1;
    public int NextClienteId() => Clientes.Count > 0 ? Clientes.Max(c => c.Id) + 1 : 1;
    public int NextUsuarioId() => Usuarios.Count > 0 ? Usuarios.Max(u => u.Id) + 1 : 1;
    public int NextOrdenId() => Ordenes.Count > 0 ? Ordenes.Max(o => o.Id) + 1 : 1;
    public int NextLevantamientoId() => Levantamientos.Count > 0 ? Levantamientos.Max(l => l.Id) + 1 : 1;
    public int NextServicioId() => Servicios.Count > 0 ? Servicios.Max(s => s.Id) + 1 : 1;
    public int NextCotizacionId() => Cotizaciones.Count > 0 ? Cotizaciones.Max(c => c.Id) + 1 : 1;
    public int NextCobroId() => Cobros.Count > 0 ? Cobros.Max(c => c.Id) + 1 : 1;
    public int NextArticuloId() => Inventario.Count > 0 ? Inventario.Max(a => a.Id) + 1 : 1;
    public int NextProveedorId() => Proveedores.Count > 0 ? Proveedores.Max(p => p.Id) + 1 : 1;

    public static string FormatFecha(string? iso)
    {
        if (string.IsNullOrEmpty(iso)) return "—";
        var parts = iso.Split('-');
        if (parts.Length != 3) return iso;
        return $"{parts[2]}/{parts[1]}/{parts[0]}";
    }

    public static string FormatFechaLarga(string? iso)
    {
        if (string.IsNullOrEmpty(iso)) return "—";
        string[] meses = { "enero", "febrero", "marzo", "abril", "mayo", "junio", "julio", "agosto", "septiembre", "octubre", "noviembre", "diciembre" };
        var parts = iso.Split('-');
        if (parts.Length != 3) return iso;
        return $"{int.Parse(parts[2])} de {meses[int.Parse(parts[1]) - 1]} de {parts[0]}";
    }

    public static string FormatHora12(string? time)
    {
        if (string.IsNullOrEmpty(time)) return "—";
        var parts = time.Split(':');
        if (parts.Length < 2) return time;
        int h = int.Parse(parts[0]);
        int m = int.Parse(parts[1]);
        string ap = h >= 12 ? "PM" : "AM";
        int hh = h % 12;
        if (hh == 0) hh = 12;
        return $"{hh:D2}:{m:D2} {ap}";
    }

    public static string AddMinutes(string? time, int addMin)
    {
        if (string.IsNullOrEmpty(time)) return "—";
        var parts = time.Split(':');
        if (parts.Length < 2) return time;
        int h = int.Parse(parts[0]);
        int m = int.Parse(parts[1]);
        m += addMin;
        h += m / 60;
        m = m % 60;
        return $"{h % 24:D2}:{m:D2}";
    }

    public static (string Bg, string Fg) GetBadgeColors(string estado) => estado switch
    {
        "Pendiente" => ("#FEF3C7", "#92400E"),
        "En proceso" => ("#DBEAFE", "#1E40AF"),
        "En Espera Administración" => ("#E0E7FF", "#3730A3"),
        "Finalizada" => ("#D1FAE5", "#065F46"),
        "Cancelada" => ("#FEE2E2", "#991B1B"),
        "Activo" => ("#D1FAE5", "#065F46"),
        "Inactivo" => ("#FEE2E2", "#991B1B"),
        "Gerente" => ("#DBEAFE", "#1E40AF"),
        "Técnico" => ("#FEF3C7", "#92400E"),
        _ => ("#F3F4F6", "#374151")
    };
}
