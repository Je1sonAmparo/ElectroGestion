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
        return await db.OrdenesTrabajo.AsNoTracking().OrderByDescending(o => o.Id).ToListAsync();
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
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ActualizarAsync(int id, List<string> trabajos, List<string> herramientas, string observaciones)
    {
        if (!appState.EsGerente) return false;
        await using var db = await contextFactory.CreateDbContextAsync();
        var orden = await db.OrdenesTrabajo.FindAsync(id);
        if (orden is null || await SolicitudFinalizadaAsync(db, orden.SolicitudId) ||
            await db.Cobros.AnyAsync(c => c.OrdenTrabajoId == id)) return false;
        orden.Trabajos = LimpiarLista(trabajos);
        orden.Herramientas = LimpiarLista(herramientas);
        orden.Observaciones = observaciones.Trim();
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CambiarEstadoAsync(int id, string estado)
    {
        if (!appState.IsAuthenticated || estado is not ("En proceso" or "Pendiente de cobro" or "Finalizada")) return false;
        if (!appState.EsGerente && estado != "Pendiente de cobro") return false;
        await using var db = await contextFactory.CreateDbContextAsync();
        var orden = await db.OrdenesTrabajo.FindAsync(id);
        if (orden is null || orden.Estado == estado) return false;
        if (estado == "En proceso" && await db.Cobros.AnyAsync(c => c.OrdenTrabajoId == id)) return false;
        if (await SolicitudFinalizadaAsync(db, orden.SolicitudId)) return false;
        orden.Estado = estado;
        var estadoSolicitud = await db.EstadosSolicitud.FirstOrDefaultAsync(e => e.Nombre == estado);
        var solicitud = await db.Solicitudes.FirstOrDefaultAsync(s => s.Id == orden.SolicitudId);
        if (estadoSolicitud is not null && solicitud is not null) solicitud.EstadoSolicitudId = estadoSolicitud.Id;
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> EliminarAsync(int id)
    {
        if (!appState.EsGerente) return false;
        await using var db = await contextFactory.CreateDbContextAsync();
        var orden = await db.OrdenesTrabajo.FindAsync(id);
        if (orden is null || await SolicitudFinalizadaAsync(db, orden.SolicitudId) ||
            await db.Cobros.AnyAsync(c => c.OrdenTrabajoId == id)) return false;
        db.OrdenesTrabajo.Remove(orden);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> TieneCobroAsync(int id)
    {
        if (!appState.EsGerente) return false;
        await using var db = await contextFactory.CreateDbContextAsync();
        return await db.Cobros.AnyAsync(c => c.OrdenTrabajoId == id);
    }

    private static Task<bool> SolicitudFinalizadaAsync(Contexto db, int solicitudId) =>
        db.Solicitudes.Where(s => s.Id == solicitudId)
            .AnyAsync(s => s.EstadoSolicitud != null && s.EstadoSolicitud.Nombre == "Finalizada");

    private static List<string> LimpiarLista(IEnumerable<string> valores) => valores
        .Select(x => x.Trim()).Where(x => x.Length > 0).ToList();
}
