using ElectroGestion.Context;
using ElectroGestion.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace ElectroGestion.Services;

public sealed record DemoSeedResult(int Solicitudes, int Cotizaciones, int Ordenes, int Cobros, int Ventas, int Productos);

public sealed class DemoDataSeeder(IDbContextFactory<Contexto> contextFactory)
{
    private enum Flujo
    {
        Pendiente, EnProceso, Administracion, Revision, Enviada, Rechazada,
        Aprobada, OrdenEnProceso, PendienteCobro, Finalizada, Cancelada
    }

    private sealed record Plan(Flujo Flujo, int MesesAtras);
    private sealed record ProductoBase(string Codigo, string Nombre, string Categoria, int Existencia, decimal Precio);

    private static readonly ProductoBase[] ProductosDemo =
    [
        new("ART-001", "Cable de Cobre", "Materiales", 500, 15m),
        new("ART-002", "Transformador 50kVA", "Equipos", 10, 25000m),
        new("ART-003", "Breaker 60A", "Protección eléctrica", 55, 1250m),
        new("ART-004", "UPS 6 kVA", "Equipos", 25, 58000m),
        new("ART-005", "Cable THHN #6", "Materiales", 800, 92m),
        new("ART-006", "Panel solar 550W", "Energía solar", 36, 12800m),
        new("ART-007", "Tubería EMT 3/4", "Canalización", 180, 450m),
        new("ART-008", "Batería 12V 120Ah", "Equipos", 40, 13500m),
        new("ART-009", "Inversor híbrido 6kW", "Energía solar", 10, 72000m),
        new("ART-010", "Canaleta metálica 3m", "Canalización", 140, 950m)
    ];

    private static readonly Plan[] Planes =
    [
        new(Flujo.Pendiente, 0), new(Flujo.Pendiente, 0), new(Flujo.Pendiente, 1), new(Flujo.Pendiente, 1), new(Flujo.Pendiente, 2),
        new(Flujo.EnProceso, 0), new(Flujo.EnProceso, 1), new(Flujo.EnProceso, 1), new(Flujo.EnProceso, 2),
        new(Flujo.Administracion, 0), new(Flujo.Administracion, 1), new(Flujo.Administracion, 2), new(Flujo.Administracion, 3),
        new(Flujo.Revision, 0), new(Flujo.Revision, 1), new(Flujo.Revision, 2),
        new(Flujo.Enviada, 0), new(Flujo.Enviada, 1), new(Flujo.Enviada, 2), new(Flujo.Enviada, 3),
        new(Flujo.Rechazada, 1), new(Flujo.Rechazada, 2), new(Flujo.Rechazada, 4),
        new(Flujo.Aprobada, 0), new(Flujo.Aprobada, 1),
        new(Flujo.OrdenEnProceso, 0), new(Flujo.OrdenEnProceso, 1),
        new(Flujo.PendienteCobro, 1), new(Flujo.PendienteCobro, 2), new(Flujo.PendienteCobro, 3),
        new(Flujo.Finalizada, 2), new(Flujo.Finalizada, 3), new(Flujo.Finalizada, 4), new(Flujo.Finalizada, 5),
        new(Flujo.Finalizada, 6), new(Flujo.Finalizada, 7), new(Flujo.Finalizada, 8), new(Flujo.Finalizada, 9),
        new(Flujo.Finalizada, 10), new(Flujo.Finalizada, 10),
        new(Flujo.Cancelada, 1), new(Flujo.Cancelada, 3), new(Flujo.Cancelada, 5)
    ];

    public async Task<DemoSeedResult> ReemplazarDatosOperativosAsync()
    {
        await using var db = await contextFactory.CreateDbContextAsync();
        VerificarBaseDeDatosLocal(db.Database.GetConnectionString());
        await using var transaccion = await db.Database.BeginTransactionAsync();

        var clientes = await db.Clientes.Include(c => c.Telefonos).OrderBy(c => c.Id).ToListAsync();
        var servicios = await db.Servicios.Where(s => s.Estado == "Activo").OrderBy(s => s.Id).ToListAsync();
        var estados = await db.EstadosSolicitud.ToDictionaryAsync(e => e.Nombre, e => e.Id);
        var tecnicos = await db.Usuarios.Where(u => u.Rol == "Técnico").OrderBy(u => u.Id).ToListAsync();
        if (clientes.Count == 0 || servicios.Count == 0 || tecnicos.Count == 0)
            throw new InvalidOperationException("Se necesitan clientes, servicios activos y al menos un técnico para generar los datos de demostración.");

        var requiredStates = new[]
        {
            "Pendiente", "En proceso", "En Espera Administración", "En revisión", "Enviada al cliente",
            "Aprobada", "Rechazada", "Pendiente de cobro", "Finalizada", "Cancelada"
        };
        var missingStates = requiredStates.Where(name => !estados.ContainsKey(name)).ToArray();
        if (missingStates.Length > 0)
            throw new InvalidOperationException($"Faltan estados de solicitud: {string.Join(", ", missingStates)}.");

        // Vacía únicamente datos operativos. Clientes, teléfonos, servicios, usuarios y estados se conservan.
        await db.AbonosCobro.IgnoreQueryFilters().ExecuteDeleteAsync();
        await db.Cobros.IgnoreQueryFilters().ExecuteDeleteAsync();
        await db.OrdenesTrabajo.IgnoreQueryFilters().ExecuteDeleteAsync();
        await db.DetallesVenta.IgnoreQueryFilters().ExecuteDeleteAsync();
        await db.Ventas.IgnoreQueryFilters().ExecuteDeleteAsync();
        await db.DetallesCotizacion.IgnoreQueryFilters().ExecuteDeleteAsync();
        await db.Cotizaciones.IgnoreQueryFilters().ExecuteDeleteAsync();
        await db.Levantamientos.IgnoreQueryFilters().ExecuteDeleteAsync();
        await db.Visitas.IgnoreQueryFilters().ExecuteDeleteAsync();
        await db.SolicitudesEventos.IgnoreQueryFilters().ExecuteDeleteAsync();
        await db.DetallesSolicitud.IgnoreQueryFilters().ExecuteDeleteAsync();
        await db.Solicitudes.IgnoreQueryFilters().ExecuteDeleteAsync();

        var articulos = await db.ArticulosInventario.IgnoreQueryFilters().ToListAsync();
        foreach (var definicion in ProductosDemo)
        {
            var articulo = articulos.FirstOrDefault(a => a.Codigo == definicion.Codigo);
            if (articulo is null)
            {
                articulo = new ArticuloInventario { Codigo = definicion.Codigo };
                articulos.Add(articulo);
                db.ArticulosInventario.Add(articulo);
            }
            articulo.Nombre = definicion.Nombre;
            articulo.Categoria = definicion.Categoria;
            articulo.Cantidad = definicion.Existencia;
            articulo.PrecioUnidad = definicion.Precio;
            articulo.IsDeleted = false;
        }
        await db.SaveChangesAsync();

        var articulosPorCodigo = articulos.ToDictionary(a => a.Codigo, StringComparer.OrdinalIgnoreCase);
        var hoy = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(-4)).Date;
        var mesActual = new DateTime(hoy.Year, hoy.Month, 1);
        var registros = new List<(Plan Plan, Solicitud Solicitud, DateTime FechaVisita)>();
        var visitas = new List<Visita>();
        var solicitudes = new List<Solicitud>();

        for (var i = 0; i < Planes.Length; i++)
        {
            var plan = Planes[i];
            var cliente = clientes[i % clientes.Count];
            var servicio = servicios[i % servicios.Count];
            var fechaSolicitud = ObtenerFechaMes(mesActual, plan.MesesAtras, i, hoy);
            var necesitaVisita = plan.Flujo != Flujo.Cancelada;
            var fechaVisita = SiguienteDiaLaborable(fechaSolicitud.AddDays(2 + i % 3));
            var tecnico = tecnicos[i % tecnicos.Count];
            var nombreEstado = ObtenerEstadoSolicitud(plan.Flujo);

            var solicitud = new Solicitud
            {
                ClienteId = cliente.Id,
                ServicioId = servicio.Id,
                Contacto = cliente.Telefono,
                Direccion = cliente.Direccion,
                FechaSolicitud = fechaSolicitud,
                EstadoSolicitudId = estados[nombreEstado],
                TecnicoAsignado = necesitaVisita ? tecnico.Nombre : "",
                FechaFin = plan.Flujo == Flujo.Finalizada ? fechaVisita.Date : null
            };

            if (plan.Flujo is Flujo.EnProceso or Flujo.Administracion or Flujo.Revision or Flujo.Enviada or Flujo.Rechazada or Flujo.Aprobada or Flujo.OrdenEnProceso or Flujo.PendienteCobro or Flujo.Finalizada)
            {
                solicitud.Levantamiento = CrearLevantamiento(i, cliente, servicio, plan.Flujo != Flujo.EnProceso);
            }

            solicitud.Eventos.Add(new SolicitudEvento
            {
                Fecha = fechaSolicitud,
                Titulo = "Solicitud registrada",
                Detalle = "Se registró la solicitud del servicio."
            });

            if (necesitaVisita)
            {
                solicitud.Eventos.Add(new SolicitudEvento
                {
                    Fecha = fechaSolicitud.AddHours(2),
                    Titulo = "Visita programada",
                    Detalle = $"Visita asignada a {tecnico.Nombre} para el {fechaVisita:dd/MM/yyyy}."
                });
                var estadoVisita = ObtenerEstadoVisita(plan.Flujo);
                visitas.Add(new Visita
                {
                    Solicitud = solicitud,
                    TecnicoId = tecnico.Id,
                    Fecha = fechaVisita,
                    Hora = ObtenerHoraVisita(i),
                    HoraFin = ObtenerHoraFinVisita(i),
                    Estado = estadoVisita
                });
            }

            if (solicitud.Levantamiento is not null)
            {
                solicitud.Eventos.Add(new SolicitudEvento
                {
                    Fecha = fechaVisita,
                    Titulo = plan.Flujo == Flujo.EnProceso ? "Levantamiento iniciado" : "Levantamiento enviado a administración",
                    Detalle = plan.Flujo == Flujo.EnProceso
                        ? "El técnico comenzó el levantamiento y guardó un borrador."
                        : "Se completó el levantamiento técnico para preparar la cotización."
                });
            }

            solicitudes.Add(solicitud);
            registros.Add((plan, solicitud, fechaVisita));
        }

        db.Solicitudes.AddRange(solicitudes);
        db.Visitas.AddRange(visitas);
        await db.SaveChangesAsync();

        var cotizacionesPorSolicitud = new Dictionary<int, Cotizacion>();
        foreach (var registro in registros.Where(x => TieneCotizacion(x.Plan.Flujo)))
        {
            var plan = registro.Plan;
            var solicitud = registro.Solicitud;
            var fechaVisita = registro.FechaVisita;
            var cliente = clientes.First(c => c.Id == solicitud.ClienteId);
            var servicio = servicios.First(s => s.Id == solicitud.ServicioId);
            var productos = ObtenerProductos(servicio.Id, articulosPorCodigo, solicitud.Id);
            var fechaCotizacion = Menor(solicitud.FechaSolicitud.AddDays(1), hoy);
            var fechaEnvio = plan.Flujo is Flujo.Enviada or Flujo.Rechazada or Flujo.Aprobada or Flujo.OrdenEnProceso or Flujo.PendienteCobro or Flujo.Finalizada
                ? Menor(fechaCotizacion.AddDays(1 + solicitud.Id % 2), hoy)
                : (DateTime?)null;
            var fechaRespuesta = plan.Flujo is Flujo.Rechazada or Flujo.Aprobada or Flujo.OrdenEnProceso or Flujo.PendienteCobro or Flujo.Finalizada
                ? Menor(fechaEnvio!.Value.AddDays(1 + solicitud.Id % 4), hoy)
                : (DateTime?)null;
            var manoObra = 4800m + solicitud.Id % 7 * 1950m;
            var subtotal = productos.Sum(d => d.Cantidad * d.PrecioUnitario) + manoObra;
            var total = subtotal + CotizacionService.CalcularImpuesto(subtotal, CotizacionService.TasaItbis);
            var cotizacion = new Cotizacion
            {
                SolicitudDbId = solicitud.Id,
                SolicitudId = $"SOL-{solicitud.Id:D4}",
                Solicitud = solicitud,
                Cliente = cliente.Nombre,
                Fecha = fechaCotizacion,
                MontoManoObra = manoObra,
                PorcentajeItbis = CotizacionService.TasaItbis,
                MontoTotal = total,
                Estado = ObtenerEstadoCotizacion(plan.Flujo),
                FechaEnvio = fechaEnvio,
                FechaRespuesta = fechaRespuesta,
                MotivoRechazo = plan.Flujo == Flujo.Rechazada
                    ? "El cliente solicitó revisar el alcance y el presupuesto antes de continuar."
                    : "",
                Detalles = productos.Select(p => new DetalleCotizacion
                {
                    ArticuloInventarioId = p.ArticuloInventarioId,
                    CodigoArticulo = p.CodigoArticulo,
                    NombreArticulo = p.NombreArticulo,
                    Cantidad = p.Cantidad,
                    PrecioUnitario = p.PrecioUnitario
                }).ToList()
            };
            db.Cotizaciones.Add(cotizacion);
            cotizacionesPorSolicitud.Add(solicitud.Id, cotizacion);
            solicitud.Eventos.Add(new SolicitudEvento
            {
                Fecha = fechaCotizacion,
                Titulo = "Cotización registrada",
                Detalle = $"Cotización en estado {cotizacion.Estado}; total RD$ {cotizacion.MontoTotal:N2}."
            });
            if (fechaEnvio.HasValue)
            {
                solicitud.Eventos.Add(new SolicitudEvento
                {
                    Fecha = fechaEnvio.Value,
                    Titulo = "Cotización enviada",
                    Detalle = "La cotización fue enviada al cliente para su revisión."
                });
            }
            if (fechaRespuesta.HasValue)
            {
                solicitud.Eventos.Add(new SolicitudEvento
                {
                    Fecha = fechaRespuesta.Value,
                    Titulo = "Respuesta a cotización",
                    Detalle = plan.Flujo == Flujo.Rechazada ? "El cliente rechazó la cotización." : "El cliente aprobó la cotización."
                });
            }
        }
        await db.SaveChangesAsync();

        var ordenesPorCotizacion = new Dictionary<int, OrdenTrabajo>();
        foreach (var registro in registros.Where(x => TieneOrden(x.Plan.Flujo)))
        {
            var plan = registro.Plan;
            var solicitud = registro.Solicitud;
            var fechaVisita = registro.FechaVisita;
            var cotizacion = cotizacionesPorSolicitud[solicitud.Id];
            var servicio = servicios.First(s => s.Id == solicitud.ServicioId);
            var orden = new OrdenTrabajo
            {
                SolicitudId = solicitud.Id,
                CotizacionId = cotizacion.Id,
                Cotizacion = cotizacion,
                Cliente = cotizacion.Cliente,
                TipoServicio = servicio.Nombre,
                Estado = ObtenerEstadoOrden(plan.Flujo),
                FechaCreacion = plan.Flujo == Flujo.Finalizada ? fechaVisita : Menor(fechaVisita, hoy),
                Trabajos = ObtenerTrabajos(servicio.Nombre),
                Herramientas = ObtenerHerramientas(solicitud.Id),
                MaterialesCotizados = cotizacion.Detalles.Select(d => new MaterialOrden { Codigo = d.CodigoArticulo, Nombre = d.NombreArticulo, Cantidad = d.Cantidad }).ToList(),
                Observaciones = "Coordinar el acceso con el cliente y documentar las pruebas de funcionamiento."
            };
            db.OrdenesTrabajo.Add(orden);
            ordenesPorCotizacion.Add(cotizacion.Id, orden);
            solicitud.Eventos.Add(new SolicitudEvento
            {
                Fecha = orden.FechaCreacion,
                Titulo = "Orden de trabajo creada",
                Detalle = $"Orden de trabajo en estado {orden.Estado}."
            });
        }
        await db.SaveChangesAsync();
        foreach (var orden in ordenesPorCotizacion.Values)
            orden.Code = $"OT-{orden.Id:D4}";
        await db.SaveChangesAsync();

        var contadorFactura = 1001;
        foreach (var registro in registros.Where(x => x.Plan.Flujo is Flujo.PendienteCobro or Flujo.Finalizada))
        {
            var plan = registro.Plan;
            var solicitud = registro.Solicitud;
            var fechaVisita = registro.FechaVisita;
            var cotizacion = cotizacionesPorSolicitud[solicitud.Id];
            var orden = ordenesPorCotizacion[cotizacion.Id];
            var fechaCobro = Menor(fechaVisita.AddDays(1), hoy);
            var cobro = new Cobro
            {
                Cliente = cotizacion.Cliente,
                CotizacionId = cotizacion.Id,
                Cotizacion = cotizacion,
                OrdenTrabajoId = orden.Id,
                OrdenTrabajo = orden,
                FacturaId = contadorFactura++,
                Fecha = fechaCobro,
                Monto = cotizacion.MontoTotal,
                MetodoPago = "Transferencia",
                Estado = plan.Flujo == Flujo.Finalizada ? "Finalizada" : "Pendiente de cobro"
            };
            if (plan.Flujo == Flujo.Finalizada)
            {
                var primerAbono = Math.Round(cobro.Monto * 0.5m, 2, MidpointRounding.AwayFromZero);
                cobro.Abonos.Add(new AbonoCobro { Fecha = Menor(fechaCobro, hoy), Monto = primerAbono, MetodoPago = "Transferencia" });
                cobro.Abonos.Add(new AbonoCobro { Fecha = Menor(fechaCobro.AddDays(2), hoy), Monto = cobro.Monto - primerAbono, MetodoPago = solicitud.Id % 2 == 0 ? "Transferencia" : "Efectivo" });
            }
            else
            {
                var porcentajePagado = solicitud.Id % 3 == 0 ? 0.65m : 0.5m;
                cobro.Abonos.Add(new AbonoCobro
                {
                    Fecha = Menor(fechaCobro, hoy),
                    Monto = Math.Round(cobro.Monto * porcentajePagado, 2, MidpointRounding.AwayFromZero),
                    MetodoPago = solicitud.Id % 2 == 0 ? "Transferencia" : "Depósito"
                });
                cobro.MetodoPago = cobro.Abonos.First().MetodoPago;
            }
            db.Cobros.Add(cobro);
            solicitud.Eventos.Add(new SolicitudEvento
            {
                Fecha = fechaCobro,
                Titulo = "Cuenta por cobrar creada",
                Detalle = $"Se abrió la cuenta por RD$ {cobro.Monto:N2}."
            });
            foreach (var abono in cobro.Abonos)
            {
                solicitud.Eventos.Add(new SolicitudEvento
                {
                    Fecha = abono.Fecha,
                    Titulo = "Abono recibido",
                    Detalle = $"Se registró un abono de RD$ {abono.Monto:N2} por {abono.MetodoPago}."
                });
            }
            if (plan.Flujo == Flujo.Finalizada)
            {
                solicitud.Eventos.Add(new SolicitudEvento
                {
                    Fecha = Menor(fechaCobro.AddDays(2), hoy),
                    Titulo = "Cobro finalizado",
                    Detalle = $"La cuenta quedó saldada por RD$ {cobro.Monto:N2}."
                });
            }
        }
        await db.SaveChangesAsync();

        var contadorNcfFiscal = 1;
        var contadorNcfConsumo = 1;
        for (var mesesAtras = 10; mesesAtras >= 0; mesesAtras--)
        {
            var ventasDelMes = mesesAtras == 0 ? 3 : 4;
            for (var j = 0; j < ventasDelMes; j++)
            {
                var fecha = ObtenerFechaVenta(mesActual, mesesAtras, j, hoy);
                var cliente = clientes[(mesesAtras * 2 + j) % clientes.Count];
                var creditoFiscal = j % 2 == 0;
                var lineas = ObtenerLineasVenta(articulosPorCodigo, mesesAtras, j);
                var venta = new Venta
                {
                    ClienteId = creditoFiscal || j != 1 ? cliente.Id : null,
                    NombreCliente = j == 1 && !creditoFiscal ? "" : cliente.Nombre,
                    DocumentoCliente = creditoFiscal ? new string(cliente.Rnc.Where(char.IsDigit).ToArray()) : "",
                    TipoComprobante = creditoFiscal ? Venta.CreditoFiscal : Venta.Consumo,
                    Estado = Venta.Registrada,
                    FechaCreacion = fecha.AddMinutes(-12),
                    FechaVenta = fecha,
                    TasaItbis = 18m,
                    Ncf = creditoFiscal ? $"DEMO-B01-{contadorNcfFiscal++:D4}" : $"DEMO-B02-{contadorNcfConsumo++:D4}",
                    Detalles = lineas.Select(linea => new DetalleVenta
                    {
                        ArticuloInventarioId = linea.Articulo.Id,
                        CodigoArticulo = linea.Articulo.Codigo,
                        NombreArticulo = linea.Articulo.Nombre,
                        Cantidad = linea.Cantidad,
                        PrecioUnitario = linea.Articulo.PrecioUnidad
                    }).ToList()
                };
                var primeraLinea = venta.Detalles[0];
                venta.ArticuloInventarioId = primeraLinea.ArticuloInventarioId;
                venta.CodigoArticulo = primeraLinea.CodigoArticulo;
                venta.NombreArticulo = primeraLinea.NombreArticulo;
                venta.Cantidad = primeraLinea.Cantidad;
                venta.PrecioUnitario = primeraLinea.PrecioUnitario;
                foreach (var linea in lineas)
                    linea.Articulo.Cantidad -= linea.Cantidad;
                db.Ventas.Add(venta);
            }
        }

        var articuloBorrador = articulosPorCodigo["ART-003"];
        var borrador = CrearVentaSimple(articuloBorrador, Venta.Borrador, null, null, null, hoy.AddHours(-3), "DEMO-B02-9998");
        db.Ventas.Add(borrador);
        var articuloAnulado = articulosPorCodigo["ART-001"];
        db.Ventas.Add(CrearVentaSimple(articuloAnulado, Venta.Anulada, clientes[2].Id, clientes[2].Nombre,
            new string(clientes[2].Rnc.Where(char.IsDigit).ToArray()), hoy.AddDays(-8), "DEMO-B01-9999"));
        await db.SaveChangesAsync();

        await transaccion.CommitAsync();
        return new DemoSeedResult(solicitudes.Count, cotizacionesPorSolicitud.Count, ordenesPorCotizacion.Count,
            await db.Cobros.CountAsync(), await db.Ventas.CountAsync(), articulos.Count);
    }

    private static void VerificarBaseDeDatosLocal(string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("No se encontró una cadena de conexión.");
        var conexion = new SqlConnectionStringBuilder(connectionString);
        var servidor = conexion.DataSource;
        var local = servidor.Equals(".", StringComparison.OrdinalIgnoreCase) ||
                    servidor.Equals("(local)", StringComparison.OrdinalIgnoreCase) ||
                    servidor.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
                    servidor.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase) ||
                    servidor.StartsWith(".\\", StringComparison.OrdinalIgnoreCase) ||
                    servidor.StartsWith("localhost\\", StringComparison.OrdinalIgnoreCase);
        if (!local || !conexion.InitialCatalog.Equals("ElectroGestionDB", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("La carga de demostración solo se permite en la base local ElectroGestionDB.");
    }

    private static DateTime ObtenerFechaMes(DateTime mesActual, int mesesAtras, int indice, DateTime hoy)
    {
        var mes = mesActual.AddMonths(-mesesAtras);
        var dia = mesesAtras == 0 ? Math.Max(1, hoy.Day - 3 + indice % 2) : Math.Min(4 + indice * 3 % 22, DateTime.DaysInMonth(mes.Year, mes.Month));
        return new DateTime(mes.Year, mes.Month, dia, 9 + indice % 7, 0, 0);
    }

    private static DateTime ObtenerFechaVenta(DateTime mesActual, int mesesAtras, int indice, DateTime hoy)
    {
        var mes = mesActual.AddMonths(-mesesAtras);
        var dia = mesesAtras == 0 ? Math.Max(1, hoy.Day - 3 + indice) : Math.Min(6 + indice * 5, DateTime.DaysInMonth(mes.Year, mes.Month));
        return new DateTime(mes.Year, mes.Month, dia, 10 + indice % 7, indice * 7 % 60, 0);
    }

    private static DateTime SiguienteDiaLaborable(DateTime fecha)
    {
        while (fecha.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday) fecha = fecha.AddDays(1);
        return fecha;
    }

    private static DateTime Menor(DateTime fecha, DateTime maximo) => fecha > maximo ? maximo : fecha;

    private static string ObtenerEstadoSolicitud(Flujo flujo) => flujo switch
    {
        Flujo.Pendiente => "Pendiente",
        Flujo.EnProceso or Flujo.OrdenEnProceso => "En proceso",
        Flujo.Administracion => "En Espera Administración",
        Flujo.Revision => "En revisión",
        Flujo.Enviada => "Enviada al cliente",
        Flujo.Rechazada => "Rechazada",
        Flujo.Aprobada => "Aprobada",
        Flujo.PendienteCobro => "Pendiente de cobro",
        Flujo.Finalizada => "Finalizada",
        Flujo.Cancelada => "Cancelada",
        _ => throw new ArgumentOutOfRangeException(nameof(flujo))
    };

    private static string ObtenerEstadoCotizacion(Flujo flujo) => flujo switch
    {
        Flujo.Revision => "En revisión",
        Flujo.Enviada => "Enviada al cliente",
        Flujo.Rechazada => "Rechazada",
        Flujo.Aprobada or Flujo.OrdenEnProceso or Flujo.PendienteCobro or Flujo.Finalizada => "Aprobada",
        _ => throw new ArgumentOutOfRangeException(nameof(flujo))
    };

    private static string ObtenerEstadoOrden(Flujo flujo) => flujo switch
    {
        Flujo.OrdenEnProceso => "En proceso",
        Flujo.PendienteCobro => "Pendiente de cobro",
        Flujo.Finalizada => "Finalizada",
        _ => throw new ArgumentOutOfRangeException(nameof(flujo))
    };

    private static string ObtenerEstadoVisita(Flujo flujo) => flujo switch
    {
        Flujo.Pendiente => "Programada",
        Flujo.EnProceso or Flujo.OrdenEnProceso => "En proceso",
        Flujo.Cancelada => "Cancelada",
        Flujo.PendienteCobro or Flujo.Finalizada => "Finalizada",
        _ => "En Espera Administración"
    };

    private static bool TieneCotizacion(Flujo flujo) => flujo is Flujo.Revision or Flujo.Enviada or Flujo.Rechazada or Flujo.Aprobada or Flujo.OrdenEnProceso or Flujo.PendienteCobro or Flujo.Finalizada;
    private static bool TieneOrden(Flujo flujo) => flujo is Flujo.OrdenEnProceso or Flujo.PendienteCobro or Flujo.Finalizada;

    private static Levantamiento CrearLevantamiento(int indice, Cliente cliente, Servicio servicio, bool completo) => new()
    {
        DisponibilidadEspacios = $"Área de trabajo disponible en horario coordinado con {cliente.Nombre}.",
        CargasElectricas = $"Carga revisada para el servicio de {servicio.Nombre}; circuitos identificados.",
        EstabilidadTension = "Tensión medida dentro del rango operativo durante la inspección.",
        RecorridoTuberias = "Recorrido principal verificado; requiere fijación y revisión de tramos existentes.",
        RecorridoCableado = "Tendido y puntos de conexión identificados en el área intervenida.",
        Observaciones = $"Levantamiento de campo #{indice + 1}: se recomienda coordinar el acceso y validar los materiales antes de iniciar.",
        FotografiasJson = "[]",
        BorradorGuardado = !completo
    };

    private static List<DetalleCotizacion> ObtenerProductos(int servicioId, Dictionary<string, ArticuloInventario> articulos, int semilla)
    {
        var codigos = servicioId switch
        {
            1 => new[] { "ART-006", "ART-009", "ART-005" },
            2 => new[] { "ART-004", "ART-008", "ART-001" },
            3 => new[] { "ART-003", "ART-005", "ART-007" },
            4 => new[] { "ART-005", "ART-007", "ART-010" },
            _ => new[] { "ART-002", "ART-003", "ART-001" }
        };
        return codigos.Take(2 + semilla % 2).Select((codigo, indice) =>
        {
            var articulo = articulos[codigo];
            return new DetalleCotizacion
            {
                ArticuloInventarioId = articulo.Id,
                CodigoArticulo = articulo.Codigo,
                NombreArticulo = articulo.Nombre,
                Cantidad = 1 + (semilla + indice) % (indice == 0 ? 3 : 5),
                PrecioUnitario = articulo.PrecioUnidad
            };
        }).ToList();
    }

    private static List<(ArticuloInventario Articulo, int Cantidad)> ObtenerLineasVenta(
        Dictionary<string, ArticuloInventario> articulos, int mesesAtras, int indice)
    {
        var codigos = new[] { "ART-001", "ART-003", "ART-005", "ART-006", "ART-007", "ART-008", "ART-010", "ART-002", "ART-004", "ART-009" };
        var primero = articulos[codigos[(mesesAtras * 3 + indice * 2) % codigos.Length]];
        var cantidadPrimera = primero.Cantidad > 1 ? 1 + (mesesAtras + indice) % Math.Min(3, primero.Cantidad) : 1;
        var lineas = new List<(ArticuloInventario Articulo, int Cantidad)> { (primero, cantidadPrimera) };
        if (indice == 2)
        {
            var segundo = articulos[codigos[(mesesAtras * 3 + indice * 2 + 4) % codigos.Length]];
            if (segundo.Id != primero.Id)
                lineas.Add((segundo, 1 + mesesAtras % Math.Min(3, Math.Max(1, segundo.Cantidad))));
        }
        return lineas;
    }

    private static List<string> ObtenerTrabajos(string servicio) =>
    [
        $"Inspección y preparación para {servicio.ToLowerInvariant()}",
        "Instalación de materiales y revisión de conexiones",
        "Pruebas eléctricas y entrega de recomendaciones"
    ];

    private static List<string> ObtenerHerramientas(int indice) => indice % 2 == 0
        ? ["Multímetro digital", "Taladro percutor", "Juego de destornilladores aislados"]
        : ["Pinza amperimétrica", "Pelacables", "Escalera dieléctrica"];

    private static string ObtenerHoraVisita(int indice)
    {
        var hora = 8 + (indice % 5) * 2;
        return $"{hora:D2}:00";
    }

    private static string ObtenerHoraFinVisita(int indice)
    {
        var hora = 9 + (indice % 5) * 2;
        return $"{hora:D2}:30";
    }

    private static Venta CrearVentaSimple(ArticuloInventario articulo, string estado, int? clienteId, string? nombre, string? documento, DateTime fecha, string ncf)
    {
        var detalle = new DetalleVenta
        {
            ArticuloInventarioId = articulo.Id,
            CodigoArticulo = articulo.Codigo,
            NombreArticulo = articulo.Nombre,
            Cantidad = 1,
            PrecioUnitario = articulo.PrecioUnidad
        };
        return new Venta
        {
            ArticuloInventarioId = articulo.Id,
            CodigoArticulo = articulo.Codigo,
            NombreArticulo = articulo.Nombre,
            ClienteId = clienteId,
            NombreCliente = nombre ?? "",
            DocumentoCliente = documento ?? "",
            TipoComprobante = clienteId.HasValue ? Venta.CreditoFiscal : Venta.Consumo,
            Estado = estado,
            FechaCreacion = fecha,
            FechaVenta = estado == Venta.Borrador ? null : fecha,
            Ncf = ncf,
            Cantidad = 1,
            PrecioUnitario = articulo.PrecioUnidad,
            TasaItbis = 18m,
            Detalles = [detalle]
        };
    }
}
