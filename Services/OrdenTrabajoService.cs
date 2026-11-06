using ElectroGestion.Context;
using ElectroGestion.Models;
using Microsoft.EntityFrameworkCore;

namespace ElectroGestion.Services;

public class OrdenTrabajoService(IDbContextFactory<Contexto> contextFactory, AppStateService appState)
{
    public async Task<List<OrdenTrabajo>> ObtenerTodasAsync()
    {
        if (!appState.IsAuthenticated) return [];
        await using var db = await contextFactory.CreateDbContextAsync();
        return await db.OrdenesTrabajo.AsNoTracking()
            .OrderBy(o => o.Estado == "En proceso" ? 0 : o.Estado == "Pendiente de cobro" ? 1 : o.Estado == "Finalizada" ? 2 : 3)
            .ThenBy(o => o.FechaCreacion)
            .ThenBy(o => o.Id)
            .ToListAsync();
    }

    public async Task<List<Cotizacion>> ObtenerCotizacionesDisponiblesAsync()
    {
        if (!appState.EsGerente) return [];
        await using var db = await contextFactory.CreateDbContextAsync();
        return await db.Cotizaciones.AsNoTracking()
            .Where(c => c.Estado == "Aprobada" && c.SolicitudDbId.HasValue &&
                !db.OrdenesTrabajo.Any(o => o.CotizacionId == c.Id))
            .Include(c => c.Detalles)
            .OrderByDescending(c => c.Id).ToListAsync();
    }

    public async Task<Cotizacion?> ObtenerCotizacionDeOrdenAsync(int ordenId)
    {
        if (!appState.EsGerente) return null;
        await using var db = await contextFactory.CreateDbContextAsync();
        return await db.Cotizaciones.AsNoTracking()
            .Where(c => db.OrdenesTrabajo.Any(o => o.Id == ordenId && o.CotizacionId == c.Id))
            .Include(c => c.Detalles)
            .Include(c => c.Solicitud).ThenInclude(s => s!.Cliente)
            .Include(c => c.Solicitud).ThenInclude(s => s!.Servicio)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> CrearAsync(int cotizacionId, List<string> trabajos, List<string> herramientas, string observaciones)
    {
        if (!appState.EsGerente) return false;
        await using var db = await contextFactory.CreateDbContextAsync();
        var cotizacion = await db.Cotizaciones.Include(c => c.Detalles).Include(c => c.Solicitud)
            .ThenInclude(s => s!.Servicio)
            .FirstOrDefaultAsync(c => c.Id == cotizacionId && c.Estado == "Aprobada" && c.SolicitudDbId.HasValue);
        if (cotizacion is null || await db.OrdenesTrabajo.AnyAsync(o => o.CotizacionId == cotizacionId) ||
            await SolicitudFinalizadaAsync(db, cotizacion.SolicitudDbId!.Value)) return false;

        var orden = new OrdenTrabajo
        {
            CotizacionId = cotizacion.Id,
            SolicitudId = cotizacion.SolicitudDbId!.Value,
            Cliente = cotizacion.Cliente,
            TipoServicio = cotizacion.Solicitud?.Servicio?.Nombre ?? "",
            Estado = "En proceso",
            FechaCreacion = DateTime.Now,
            Trabajos = LimpiarLista(trabajos),
            Herramientas = LimpiarLista(herramientas),
            MaterialesCotizados = cotizacion.Detalles.Select(d => new MaterialOrden
            {
                Codigo = d.CodigoArticulo, Nombre = d.NombreArticulo, Cantidad = d.Cantidad
            }).ToList(),
            Observaciones = observaciones.Trim()
        };
        db.OrdenesTrabajo.Add(orden);
        var solicitud = await db.Solicitudes.FirstAsync(s => s.Id == orden.SolicitudId);
        solicitud.EstadoSolicitudId = await db.EstadosSolicitud.Where(e => e.Nombre == "En proceso").Select(e => e.Id).FirstAsync();
        await db.SaveChangesAsync();
        orden.Code = $"OT-{orden.Id:D4}";
        var cobroExistente = await db.Cobros.FirstOrDefaultAsync(c => c.CotizacionId == cotizacionId);
        if (cobroExistente is not null)
            cobroExistente.OrdenTrabajoId = orden.Id;
        RegistroEventosSolicitud.Agregar(db, orden.SolicitudId, "Orden de trabajo creada", $"Se creó {orden.Code} en estado En proceso.");
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ActualizarAsync(int id, List<string> trabajos, List<string> herramientas, string observaciones)
    {
        if (!appState.EsGerente) return false;
        await using var db = await contextFactory.CreateDbContextAsync();
        var orden = await db.OrdenesTrabajo.FirstOrDefaultAsync(o => o.Id == id);
        if (orden is null || await SolicitudFinalizadaAsync(db, orden.SolicitudId)) return false;
        orden.Trabajos = LimpiarLista(trabajos);
        orden.Herramientas = LimpiarLista(herramientas);
        orden.Observaciones = observaciones.Trim();
        RegistroEventosSolicitud.Agregar(db, orden.SolicitudId, "Orden de trabajo actualizada", "Se actualizaron tareas, herramientas o instrucciones de campo.");
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CambiarEstadoAsync(int id, string estado)
    {
        if (!appState.IsAuthenticated || estado is not ("En proceso" or "Pendiente de cobro" or "Finalizada")) return false;
        if (!appState.EsGerente && estado != "Pendiente de cobro") return false;
        await using var db = await contextFactory.CreateDbContextAsync();
        var orden = await db.OrdenesTrabajo.FirstOrDefaultAsync(o => o.Id == id);
        if (orden is null || orden.Estado == estado) return false;
        if (estado == "En proceso" && await db.Cobros.AnyAsync(c => c.OrdenTrabajoId == id)) return false;
        if (await SolicitudFinalizadaAsync(db, orden.SolicitudId)) return false;
        var estadoReal = estado;
        if (estado == "Pendiente de cobro")
        {
            var cotizacionId = orden.CotizacionId;
            var cobro = await db.Cobros.Include(c => c.Abonos).FirstOrDefaultAsync(c => c.CotizacionId == cotizacionId);
            if (cobro?.Estado == "Finalizada" && cobro.MontoPagado >= cobro.Monto)
                estadoReal = "Finalizada";
        }
        orden.Estado = estadoReal;
        var estadoSolicitud = await db.EstadosSolicitud.FirstOrDefaultAsync(e => e.Nombre == estadoReal);
        var solicitud = await db.Solicitudes.FirstOrDefaultAsync(s => s.Id == orden.SolicitudId);
        if (estadoSolicitud is not null && solicitud is not null)
        {
            solicitud.EstadoSolicitudId = estadoSolicitud.Id;
            if (estadoReal == "Finalizada") solicitud.FechaFin = DateTime.Today;
        }
        RegistroEventosSolicitud.Agregar(db, orden.SolicitudId, "Estado de orden actualizado", $"La orden pasó a {estadoReal}.");
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> EliminarAsync(int id)
    {
        if (!appState.EsGerente) return false;
        await using var db = await contextFactory.CreateDbContextAsync();
        var orden = await db.OrdenesTrabajo.FirstOrDefaultAsync(o => o.Id == id);
        if (orden is null || await SolicitudFinalizadaAsync(db, orden.SolicitudId) ||
            await db.Cobros.IgnoreQueryFilters().AnyAsync(c => c.OrdenTrabajoId == id)) return false;
        RegistroEventosSolicitud.Agregar(db, orden.SolicitudId, "Orden de trabajo eliminada", $"Se eliminó {orden.Code}.");
        orden.IsDeleted = true;
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<HashSet<int>> ObtenerElegiblesAsync(IEnumerable<int> ids)
    {
        var elegibles = new HashSet<int>();
        if (!appState.EsGerente) return elegibles;
        var idList = ids.Distinct().ToList();
        if (idList.Count == 0) return elegibles;
        await using var db = await contextFactory.CreateDbContextAsync();
        var ordenes = await db.OrdenesTrabajo.AsNoTracking()
            .Where(o => idList.Contains(o.Id))
            .Select(o => new
            {
                o.Id,
                Finalizada = db.Solicitudes.Any(s => s.Id == o.SolicitudId &&
                    s.EstadoSolicitud != null && s.EstadoSolicitud.Nombre == "Finalizada"),
                ConCobro = db.Cobros.IgnoreQueryFilters().Any(c => c.OrdenTrabajoId == o.Id)
            })
            .ToListAsync();
        foreach (var orden in ordenes)
            if (!orden.Finalizada && !orden.ConCobro)
                elegibles.Add(orden.Id);
        return elegibles;
    }

    public async Task<int> EliminarVariosAsync(IReadOnlyCollection<int> ids)
    {
        var idList = ids.Distinct().ToList();
        if (!appState.EsGerente || idList.Count == 0) return 0;
        await using var db = await contextFactory.CreateDbContextAsync();
        await using var transaccion = await db.Database.BeginTransactionAsync();
        try
        {
            var ordenes = await db.OrdenesTrabajo.Where(o => idList.Contains(o.Id)).ToListAsync();
            var elegibles = new List<OrdenTrabajo>();
            foreach (var orden in ordenes)
            {
                if (await SolicitudFinalizadaAsync(db, orden.SolicitudId) ||
                    await db.Cobros.IgnoreQueryFilters().AnyAsync(c => c.OrdenTrabajoId == orden.Id)) continue;
                elegibles.Add(orden);
            }
            if (elegibles.Count == 0)
            {
                await transaccion.RollbackAsync();
                return 0;
            }
            foreach (var orden in elegibles)
            {
                RegistroEventosSolicitud.Agregar(db, orden.SolicitudId, "Orden de trabajo eliminada", $"Se eliminó {orden.Code}.");
                orden.IsDeleted = true;
            }
            await db.SaveChangesAsync();
            await transaccion.CommitAsync();
            return elegibles.Count;
        }
        catch
        {
            await transaccion.RollbackAsync();
            return 0;
        }
    }

    public async Task<bool> TieneCobroAsync(int id)
    {
        if (!appState.EsGerente) return false;
        await using var db = await contextFactory.CreateDbContextAsync();
        return await db.Cobros.AnyAsync(c => c.OrdenTrabajoId == id);
    }

    public async Task<bool> PuedeModificarAsync(int id)
    {
        if (!appState.EsGerente) return false;
        await using var db = await contextFactory.CreateDbContextAsync();
        var orden = await db.OrdenesTrabajo.AsNoTracking().FirstOrDefaultAsync(o => o.Id == id);
        return orden is not null && !await SolicitudFinalizadaAsync(db, orden.SolicitudId);
    }

    private static Task<bool> SolicitudFinalizadaAsync(Contexto db, int solicitudId) =>
        db.Solicitudes.Where(s => s.Id == solicitudId)
            .AnyAsync(s => s.EstadoSolicitud != null && s.EstadoSolicitud.Nombre == "Finalizada");

    private static List<string> LimpiarLista(IEnumerable<string> valores) => valores
        .Select(x => x.Trim()).Where(x => x.Length > 0).ToList();
}
