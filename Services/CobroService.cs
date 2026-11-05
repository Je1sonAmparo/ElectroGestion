using ElectroGestion.Context;
using ElectroGestion.Models;
using Microsoft.EntityFrameworkCore;

namespace ElectroGestion.Services;

public class CobroService(IDbContextFactory<Contexto> factory, AppStateService appState)
{
    public async Task<List<Cobro>> ObtenerTodosAsync()
    {
        await using var db = await factory.CreateDbContextAsync();
        return await db.Cobros.AsNoTracking()
            .Include(c => c.OrdenTrabajo).ThenInclude(o => o!.Cotizacion).ThenInclude(q => q!.Detalles)
            .Include(c => c.OrdenTrabajo).ThenInclude(o => o!.Cotizacion).ThenInclude(q => q!.Solicitud).ThenInclude(s => s!.Cliente)
            .Include(c => c.OrdenTrabajo).ThenInclude(o => o!.Cotizacion).ThenInclude(q => q!.Solicitud).ThenInclude(s => s!.Servicio)
            .OrderByDescending(c => c.Id).ToListAsync();
    }

    public async Task<List<OrdenTrabajo>> ObtenerOrdenesCobrablesAsync()
    {
        if (!appState.EsGerente) return [];
        await using var db = await factory.CreateDbContextAsync();
        return await db.OrdenesTrabajo.AsNoTracking()
            .Include(o => o.Cotizacion)
            .Where(o => o.Estado == "Pendiente de cobro" && !db.Cobros.Any(c => c.OrdenTrabajoId == o.Id))
            .OrderByDescending(o => o.Id).ToListAsync();
    }

    public async Task<bool> CrearAsync(int ordenId, string metodoPago)
    {
        if (!appState.EsGerente || string.IsNullOrWhiteSpace(metodoPago)) return false;
        await using var db = await factory.CreateDbContextAsync();
        var orden = await db.OrdenesTrabajo.Include(o => o.Cotizacion).FirstOrDefaultAsync(o => o.Id == ordenId && o.Estado == "Pendiente de cobro");
        if (orden is null || orden.Cotizacion is null || await db.Cobros.AnyAsync(c => c.OrdenTrabajoId == ordenId)) return false;
        var cobro = new Cobro { OrdenTrabajoId = orden.Id, Cliente = orden.Cliente, FacturaId = 0, Fecha = DateTime.Now, MetodoPago = metodoPago.Trim(), Monto = orden.Cotizacion.MontoTotal, Estado = "Pendiente de cobro" };
        db.Cobros.Add(cobro);
        RegistroEventosSolicitud.Agregar(db, orden.SolicitudId, "Cobro creado", $"Se registró el cobro de {orden.Code} por RD$ {cobro.Monto:N2}; estado: Pendiente de cobro.");
        await db.SaveChangesAsync();
        cobro.FacturaId = 1000 + cobro.Id;
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CambiarEstadoAsync(int id, string estado)
    {
        if (!appState.EsGerente || estado != "Finalizada") return false;
        await using var db = await factory.CreateDbContextAsync();
        var cobro = await db.Cobros.Include(c => c.OrdenTrabajo).FirstOrDefaultAsync(c => c.Id == id);
        if (cobro is null || cobro.Estado is not ("Pendiente de cobro" or "Finalizada")) return false;
        cobro.Estado = estado;
        if (estado == "Finalizada" && cobro.OrdenTrabajo is not null)
        {
            cobro.OrdenTrabajo.Estado = "Finalizada";
            var solicitud = await db.Solicitudes.FirstOrDefaultAsync(s => s.Id == cobro.OrdenTrabajo.SolicitudId);
            var estadoFinal = await db.EstadosSolicitud.FirstOrDefaultAsync(e => e.Nombre == "Finalizada");
            if (solicitud is not null && estadoFinal is not null)
            {
                solicitud.EstadoSolicitudId = estadoFinal.Id;
                solicitud.FechaFin = DateTime.Today;
            }
            RegistroEventosSolicitud.Agregar(db, cobro.OrdenTrabajo.SolicitudId, "Cobro finalizado", $"Se registró el pago final de la orden por RD$ {cobro.Monto:N2}.");
        }
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ReabrirAsync(int id)
    {
        if (!appState.EsGerente) return false;
        await using var db = await factory.CreateDbContextAsync();
        var cobro = await db.Cobros.Include(c => c.OrdenTrabajo)
            .FirstOrDefaultAsync(c => c.Id == id && c.Estado == "Finalizada");
        if (cobro?.OrdenTrabajo is null) return false;

        var estadoPendiente = await db.EstadosSolicitud.FirstOrDefaultAsync(e => e.Nombre == "Pendiente de cobro");
        var solicitud = await db.Solicitudes.FirstOrDefaultAsync(s => s.Id == cobro.OrdenTrabajo.SolicitudId);
        if (estadoPendiente is null || solicitud is null) return false;

        cobro.Estado = "Pendiente de cobro";
        cobro.OrdenTrabajo.Estado = "Pendiente de cobro";
        solicitud.EstadoSolicitudId = estadoPendiente.Id;
        solicitud.FechaFin = null;
        RegistroEventosSolicitud.Agregar(db, solicitud.Id, "Cobro reabierto", "El cobro, la orden y la solicitud volvieron a Pendiente de cobro.");
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> EliminarAsync(int id)
    {
        if (!appState.EsGerente) return false;
        await using var db = await factory.CreateDbContextAsync();
        var cobro = await db.Cobros.Include(c => c.OrdenTrabajo).FirstOrDefaultAsync(c => c.Id == id);
        if (cobro is null || cobro.Estado is not ("Pendiente de cobro" or "Finalizada")) return false;

        if (cobro.Estado == "Finalizada" && cobro.OrdenTrabajo is not null)
        {
            var estadoPendiente = await db.EstadosSolicitud.FirstOrDefaultAsync(e => e.Nombre == "Pendiente de cobro");
            var solicitud = await db.Solicitudes.FirstOrDefaultAsync(s => s.Id == cobro.OrdenTrabajo.SolicitudId);
            if (estadoPendiente is null || solicitud is null) return false;
            cobro.OrdenTrabajo.Estado = "Pendiente de cobro";
            solicitud.EstadoSolicitudId = estadoPendiente.Id;
            solicitud.FechaFin = null;
        }

        if (cobro.OrdenTrabajo is not null)
            RegistroEventosSolicitud.Agregar(db, cobro.OrdenTrabajo.SolicitudId, "Cobro eliminado", $"Se eliminó el cobro de la orden {cobro.OrdenTrabajo.Code}.");
        db.Cobros.Remove(cobro);
        await db.SaveChangesAsync();
        return true;
    }
}

