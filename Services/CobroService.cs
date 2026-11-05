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
            .Include(c => c.Abonos)
            .Include(c => c.Cotizacion).ThenInclude(q => q!.Detalles)
            .Include(c => c.Cotizacion).ThenInclude(q => q!.Solicitud).ThenInclude(s => s!.Cliente)
            .Include(c => c.Cotizacion).ThenInclude(q => q!.Solicitud).ThenInclude(s => s!.Servicio)
            .Include(c => c.OrdenTrabajo)
            .OrderByDescending(c => c.Id).ToListAsync();
    }

    public async Task<List<Cotizacion>> ObtenerCotizacionesAprobadasAsync()
    {
        if (!appState.EsGerente) return [];
        await using var db = await factory.CreateDbContextAsync();
        return await db.Cotizaciones.AsNoTracking()
            .Where(q => q.Estado == "Aprobada" && q.SolicitudDbId.HasValue &&
                q.Solicitud != null && q.Solicitud.EstadoSolicitud != null &&
                q.Solicitud.EstadoSolicitud.Nombre != "Finalizada" &&
                !db.Cobros.Any(c => c.CotizacionId == q.Id))
            .Include(q => q.Detalles)
            .Include(q => q.Solicitud).ThenInclude(s => s!.Cliente)
            .Include(q => q.Solicitud).ThenInclude(s => s!.Servicio)
            .OrderByDescending(q => q.Id).ToListAsync();
    }

    public async Task<bool> CrearAsync(int cotizacionId, decimal abonoInicial, string metodoPago)
    {
        if (!appState.EsGerente || abonoInicial < 0 || (abonoInicial > 0 && string.IsNullOrWhiteSpace(metodoPago))) return false;
        await using var db = await factory.CreateDbContextAsync();
        var cotizacion = await db.Cotizaciones
            .Include(q => q.Solicitud).ThenInclude(s => s!.Cliente)
            .Include(q => q.Solicitud).ThenInclude(s => s!.EstadoSolicitud)
            .FirstOrDefaultAsync(q => q.Id == cotizacionId && q.Estado == "Aprobada" && q.SolicitudDbId.HasValue);
        if (cotizacion is null || cotizacion.Solicitud is null || cotizacion.Solicitud.EstadoSolicitud?.Nombre == "Finalizada" ||
            abonoInicial > cotizacion.MontoTotal || await db.Cobros.AnyAsync(c => c.CotizacionId == cotizacionId)) return false;

        var orden = await db.OrdenesTrabajo.FirstOrDefaultAsync(o => o.CotizacionId == cotizacionId);
        var cobro = new Cobro
        {
            CotizacionId = cotizacion.Id,
            OrdenTrabajoId = orden?.Id,
            Cliente = cotizacion.Solicitud.Cliente?.Nombre ?? cotizacion.Cliente,
            FacturaId = 0,
            Fecha = DateTime.Now,
            Monto = cotizacion.MontoTotal,
            MetodoPago = abonoInicial > 0 ? metodoPago.Trim() : "",
            Estado = "Pendiente de cobro"
        };
        if (abonoInicial > 0)
            cobro.Abonos.Add(new AbonoCobro { Fecha = DateTime.Now, Monto = abonoInicial, MetodoPago = metodoPago.Trim() });

        db.Cobros.Add(cobro);
        var solicitudId = cotizacion.SolicitudDbId.GetValueOrDefault();
        RegistroEventosSolicitud.Agregar(db, solicitudId, "Cuenta por cobrar creada", $"Se abrió el cobro de la cotización COT-{cotizacion.Id:D5} por RD$ {cotizacion.MontoTotal:N2}.");
        if (abonoInicial > 0)
            RegistroEventosSolicitud.Agregar(db, solicitudId, "Abono recibido", $"Se registró un abono de RD$ {abonoInicial:N2}; saldo pendiente RD$ {cotizacion.MontoTotal - abonoInicial:N2}.");
        await db.SaveChangesAsync();
        cobro.FacturaId = 1000 + cobro.Id;
        await db.SaveChangesAsync();
        await CompletarOrdenSiCorrespondeAsync(db, cobro);
        return true;
    }

    public async Task<bool> RegistrarAbonoAsync(int cobroId, decimal monto, string metodoPago)
    {
        if (!appState.EsGerente || monto <= 0 || string.IsNullOrWhiteSpace(metodoPago)) return false;
        await using var db = await factory.CreateDbContextAsync();
        var cobro = await db.Cobros.Include(c => c.Abonos).Include(c => c.OrdenTrabajo)
            .FirstOrDefaultAsync(c => c.Id == cobroId);
        if (cobro is null || cobro.Estado == "Finalizada" || monto > cobro.SaldoPendiente) return false;

        cobro.Abonos.Add(new AbonoCobro { Fecha = DateTime.Now, Monto = monto, MetodoPago = metodoPago.Trim() });
        cobro.MetodoPago = metodoPago.Trim();
        cobro.Estado = "Pendiente de cobro";
        var solicitudId = cobro.CotizacionId > 0
            ? await db.Cotizaciones.Where(q => q.Id == cobro.CotizacionId).Select(q => q.SolicitudDbId).FirstOrDefaultAsync()
            : null;
        if (solicitudId.HasValue)
        {
            RegistroEventosSolicitud.Agregar(db, solicitudId.Value, "Abono recibido", $"Se registró un abono de RD$ {monto:N2}; saldo pendiente RD$ {cobro.SaldoPendiente:N2}.");
        }
        await db.SaveChangesAsync();
        await CompletarOrdenSiCorrespondeAsync(db, cobro);
        return true;
    }

    public async Task<bool> EditarAbonoAsync(int cobroId, int abonoId, decimal monto, string metodoPago)
    {
        if (!appState.EsGerente || monto <= 0 || string.IsNullOrWhiteSpace(metodoPago)) return false;
        await using var db = await factory.CreateDbContextAsync();
        var cobro = await db.Cobros.Include(c => c.Abonos)
            .FirstOrDefaultAsync(c => c.Id == cobroId && c.Estado == "Pendiente de cobro");
        var abono = cobro?.Abonos.FirstOrDefault(a => a.Id == abonoId && !a.Anulado);
        if (cobro is null || abono is null || cobro.MontoPagado - abono.Monto + monto > cobro.Monto) return false;

        var montoAnterior = abono.Monto;
        abono.Monto = monto;
        abono.MetodoPago = metodoPago.Trim();
        var metodoReciente = cobro.Abonos.Where(a => !a.Anulado).OrderByDescending(a => a.Fecha).ThenByDescending(a => a.Id).FirstOrDefault();
        if (metodoReciente is not null) cobro.MetodoPago = metodoReciente.MetodoPago;
        var solicitudId = await db.Cotizaciones.Where(q => q.Id == cobro.CotizacionId)
            .Select(q => q.SolicitudDbId).FirstOrDefaultAsync();
        if (solicitudId.HasValue)
            RegistroEventosSolicitud.Agregar(db, solicitudId.Value, "Abono actualizado", $"Se actualizó un abono de RD$ {montoAnterior:N2} a RD$ {monto:N2}; saldo pendiente RD$ {cobro.SaldoPendiente:N2}.");
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> EliminarAbonoAsync(int cobroId, int abonoId)
    {
        if (!appState.EsGerente) return false;
        await using var db = await factory.CreateDbContextAsync();
        var cobro = await db.Cobros.Include(c => c.Abonos)
            .FirstOrDefaultAsync(c => c.Id == cobroId && c.Estado == "Pendiente de cobro");
        var abono = cobro?.Abonos.FirstOrDefault(a => a.Id == abonoId && !a.Anulado);
        if (cobro is null || abono is null) return false;

        abono.Anulado = true;
        var solicitudId = await db.Cotizaciones.Where(q => q.Id == cobro.CotizacionId)
            .Select(q => q.SolicitudDbId).FirstOrDefaultAsync();
        if (solicitudId.HasValue)
            RegistroEventosSolicitud.Agregar(db, solicitudId.Value, "Abono eliminado", $"Se eliminó un abono de RD$ {abono.Monto:N2}; saldo pendiente RD$ {cobro.SaldoPendiente:N2}.");
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CambiarEstadoAsync(int id, string estado)
    {
        if (!appState.EsGerente || estado != "Finalizada") return false;
        await using var db = await factory.CreateDbContextAsync();
        var cobro = await db.Cobros.Include(c => c.Abonos).Include(c => c.OrdenTrabajo).FirstOrDefaultAsync(c => c.Id == id);
        if (cobro is null || cobro.Estado == "Finalizada" || cobro.MontoPagado < cobro.Monto) return false;
        cobro.Estado = "Finalizada";
        var solicitudId = await db.Cotizaciones.Where(q => q.Id == cobro.CotizacionId)
            .Select(q => q.SolicitudDbId).FirstOrDefaultAsync();
        if (solicitudId.HasValue)
            RegistroEventosSolicitud.Agregar(db, solicitudId.Value, "Cobro finalizado", $"La cuenta quedó saldada por RD$ {cobro.Monto:N2}.");
        await db.SaveChangesAsync();
        await CompletarOrdenSiCorrespondeAsync(db, cobro);
        return true;
    }

    public async Task<bool> ReabrirAsync(int id)
    {
        if (!appState.EsGerente) return false;
        await using var db = await factory.CreateDbContextAsync();
        var cobro = await db.Cobros.Include(c => c.Abonos).Include(c => c.OrdenTrabajo)
            .FirstOrDefaultAsync(c => c.Id == id && c.Estado == "Finalizada");
        var ultimoAbono = cobro?.Abonos.Where(a => !a.Anulado).OrderByDescending(a => a.Fecha).ThenByDescending(a => a.Id).FirstOrDefault();
        if (cobro is null || ultimoAbono is null) return false;

        var ordenCerrada = cobro.OrdenTrabajo?.Estado == "Finalizada";
        ultimoAbono.Anulado = true;
        cobro.Estado = "Pendiente de cobro";
        if (ordenCerrada && cobro.OrdenTrabajo is not null)
            cobro.OrdenTrabajo.Estado = "Pendiente de cobro";
        var solicitudId = await db.Cotizaciones.Where(q => q.Id == cobro.CotizacionId).Select(q => q.SolicitudDbId).FirstOrDefaultAsync();
        if (solicitudId.HasValue)
        {
            var solicitud = await db.Solicitudes.FirstOrDefaultAsync(s => s.Id == solicitudId.Value);
            var estadoPendiente = ordenCerrada
                ? await db.EstadosSolicitud.FirstOrDefaultAsync(e => e.Nombre == "Pendiente de cobro")
                : null;
            if (solicitud is not null && estadoPendiente is not null)
            {
                solicitud.EstadoSolicitudId = estadoPendiente.Id;
                solicitud.FechaFin = null;
            }
            RegistroEventosSolicitud.Agregar(db, solicitudId.Value, "Abono revertido", $"Se revirtió el último abono de RD$ {ultimoAbono.Monto:N2}; cuenta reabierta.");
        }
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> EliminarAsync(int id)
    {
        if (!appState.EsGerente) return false;
        await using var db = await factory.CreateDbContextAsync();
        var cobro = await db.Cobros.Include(c => c.OrdenTrabajo).Include(c => c.Abonos).FirstOrDefaultAsync(c => c.Id == id);
        if (cobro is null || cobro.Estado is not ("Pendiente de cobro" or "Finalizada")) return false;

        var solicitudId = await db.Cotizaciones.Where(q => q.Id == cobro.CotizacionId).Select(q => q.SolicitudDbId).FirstOrDefaultAsync();
        if (cobro.Estado == "Finalizada" && cobro.OrdenTrabajo is not null)
        {
            var estadoPendiente = await db.EstadosSolicitud.FirstOrDefaultAsync(e => e.Nombre == "Pendiente de cobro");
            var solicitud = solicitudId.HasValue ? await db.Solicitudes.FirstOrDefaultAsync(s => s.Id == solicitudId.Value) : null;
            if (estadoPendiente is not null && solicitud is not null)
            {
                cobro.OrdenTrabajo.Estado = "Pendiente de cobro";
                solicitud.EstadoSolicitudId = estadoPendiente.Id;
                solicitud.FechaFin = null;
            }
        }
        if (solicitudId.HasValue)
            RegistroEventosSolicitud.Agregar(db, solicitudId.Value, "Cobro eliminado", $"Se eliminó la cuenta de cobro de COT-{cobro.CotizacionId:D5}.");
        db.Cobros.Remove(cobro);
        await db.SaveChangesAsync();
        return true;
    }

    private static async Task CompletarOrdenSiCorrespondeAsync(Contexto db, Cobro cobro)
    {
        if (cobro.Estado != "Finalizada" || cobro.OrdenTrabajoId is null) return;
        var orden = cobro.OrdenTrabajo ?? await db.OrdenesTrabajo.FirstOrDefaultAsync(o => o.Id == cobro.OrdenTrabajoId.Value);
        if (orden?.Estado != "Pendiente de cobro") return;
        orden.Estado = "Finalizada";
        var solicitud = await db.Solicitudes.FirstOrDefaultAsync(s => s.Id == orden.SolicitudId);
        var estadoFinal = await db.EstadosSolicitud.FirstOrDefaultAsync(e => e.Nombre == "Finalizada");
        if (solicitud is not null && estadoFinal is not null)
        {
            solicitud.EstadoSolicitudId = estadoFinal.Id;
            solicitud.FechaFin = DateTime.Today;
            RegistroEventosSolicitud.Agregar(db, solicitud.Id, "Servicio y cobro finalizados", "La orden fue completada y el saldo quedó pagado.");
        }
        await db.SaveChangesAsync();
    }
}
