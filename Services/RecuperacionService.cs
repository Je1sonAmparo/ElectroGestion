using ElectroGestion.Context;
using ElectroGestion.Models;
using Microsoft.EntityFrameworkCore;

namespace ElectroGestion.Services;

public enum TipoRegistroRecuperable
{
    Cotizaciones,
    OrdenesTrabajo,
    Cobros,
    Clientes,
    Ventas,
    Servicios,
    Inventario
}

public enum ResultadoRecuperacion
{
    Recuperado,
    NoEncontrado,
    DependenciaEliminada,
    AccesoRestringido
}

public sealed record RegistroEliminado(
    TipoRegistroRecuperable Tipo,
    int Id,
    string Referencia,
    string Nombre,
    string Estado,
    string Descripcion);

public class RecuperacionService(IDbContextFactory<Contexto> contextFactory, AppStateService appState)
{
    public async Task<List<RegistroEliminado>> ObtenerEliminadosAsync(TipoRegistroRecuperable tipo)
    {
        if (!appState.EsAdministrador) return [];

        await using var db = await contextFactory.CreateDbContextAsync();
        return tipo switch
        {
            TipoRegistroRecuperable.Cotizaciones => (await db.Cotizaciones.IgnoreQueryFilters()
                .Where(c => c.IsDeleted)
                .Include(c => c.Detalles)
                .AsNoTracking()
                .OrderByDescending(c => c.Id)
                .ToListAsync())
                .Select(c => new RegistroEliminado(tipo, c.Id, $"COT-{c.Id:D5}", c.Cliente, c.Estado,
                    $"{c.SolicitudId} · RD$ {c.MontoTotal:N2} · {c.Detalles.Count} producto(s)"))
                .ToList(),
            TipoRegistroRecuperable.OrdenesTrabajo => (await db.OrdenesTrabajo.IgnoreQueryFilters()
                .Where(o => o.IsDeleted)
                .AsNoTracking()
                .OrderByDescending(o => o.Id)
                .ToListAsync())
                .Select(o => new RegistroEliminado(tipo, o.Id, o.Code, o.Cliente, o.Estado,
                    $"{o.TipoServicio} · Solicitud #{o.SolicitudId}"))
                .ToList(),
            TipoRegistroRecuperable.Cobros => (await db.Cobros.IgnoreQueryFilters()
                .Where(c => c.IsDeleted)
                .Include(c => c.Abonos)
                .AsNoTracking()
                .OrderByDescending(c => c.Id)
                .ToListAsync())
                .Select(c => new RegistroEliminado(tipo, c.Id, $"COB-{c.Id:D5}", c.Cliente, c.Estado,
                    $"Factura {c.FacturaId} · RD$ {c.Monto:N2} · Pagado RD$ {c.MontoPagado:N2}"))
                .ToList(),
            TipoRegistroRecuperable.Clientes => (await db.Clientes.IgnoreQueryFilters()
                .Where(c => c.IsDeleted)
                .Include(c => c.Telefonos)
                .AsNoTracking()
                .OrderByDescending(c => c.Id)
                .ToListAsync())
                .Select(c => new RegistroEliminado(tipo, c.Id, $"CLI-{c.Id:D4}", c.Nombre, "Eliminado",
                    $"{c.Rnc} · {c.Telefono}"))
                .ToList(),
            TipoRegistroRecuperable.Ventas => (await db.Ventas.IgnoreQueryFilters()
                .Where(v => v.IsDeleted)
                .Include(v => v.Detalles)
                .AsNoTracking()
                .OrderByDescending(v => v.Id)
                .ToListAsync())
                .Select(v => new RegistroEliminado(tipo, v.Id, v.Ncf ?? $"VEN-{v.Id:D5}", v.NombreCliente, v.Estado,
                    $"{v.Detalles.Count} producto(s) · RD$ {v.Total:N2}"))
                .ToList(),
            TipoRegistroRecuperable.Servicios => (await db.Servicios.IgnoreQueryFilters()
                .Where(s => s.IsDeleted)
                .AsNoTracking()
                .OrderByDescending(s => s.Id)
                .ToListAsync())
                .Select(s => new RegistroEliminado(tipo, s.Id, s.ServicioId, s.Nombre, s.Estado, s.Categoria))
                .ToList(),
            TipoRegistroRecuperable.Inventario => (await db.ArticulosInventario.IgnoreQueryFilters()
                .Where(a => a.IsDeleted)
                .AsNoTracking()
                .OrderByDescending(a => a.Id)
                .ToListAsync())
                .Select(a => new RegistroEliminado(tipo, a.Id, a.Codigo, a.Nombre, a.Estado,
                    $"{a.Categoria} · {a.Cantidad} unidades · RD$ {a.PrecioUnidad:N2}"))
                .ToList(),
            _ => throw new ArgumentOutOfRangeException(nameof(tipo), tipo, "Tipo de recuperación desconocido.")
        };
    }

    public async Task<ResultadoRecuperacion> RestaurarAsync(TipoRegistroRecuperable tipo, int id)
    {
        if (!appState.EsAdministrador) return ResultadoRecuperacion.AccesoRestringido;
        if (id <= 0) return ResultadoRecuperacion.NoEncontrado;

        await using var db = await contextFactory.CreateDbContextAsync();
        switch (tipo)
        {
            case TipoRegistroRecuperable.Cotizaciones:
            {
                var cotizacion = await db.Cotizaciones.IgnoreQueryFilters()
                    .FirstOrDefaultAsync(c => c.Id == id && c.IsDeleted);
                if (cotizacion is null) return ResultadoRecuperacion.NoEncontrado;
                if (cotizacion.SolicitudDbId.HasValue &&
                    !await db.Solicitudes.IgnoreQueryFilters().AnyAsync(s =>
                        s.Id == cotizacion.SolicitudDbId.Value && !s.IsDeleted &&
                        s.Cliente != null && !s.Cliente.IsDeleted))
                    return ResultadoRecuperacion.DependenciaEliminada;
                cotizacion.IsDeleted = false;
                break;
            }
            case TipoRegistroRecuperable.OrdenesTrabajo:
            {
                var orden = await db.OrdenesTrabajo.IgnoreQueryFilters()
                    .FirstOrDefaultAsync(o => o.Id == id && o.IsDeleted);
                if (orden is null) return ResultadoRecuperacion.NoEncontrado;
                if (!await db.Cotizaciones.IgnoreQueryFilters().AnyAsync(c =>
                        c.Id == orden.CotizacionId && !c.IsDeleted) ||
                    !await db.Solicitudes.IgnoreQueryFilters().AnyAsync(s =>
                        s.Id == orden.SolicitudId && !s.IsDeleted &&
                        s.Cliente != null && !s.Cliente.IsDeleted))
                    return ResultadoRecuperacion.DependenciaEliminada;
                orden.IsDeleted = false;
                break;
            }
            case TipoRegistroRecuperable.Cobros:
            {
                var cobro = await db.Cobros.IgnoreQueryFilters()
                    .Include(c => c.OrdenTrabajo)
                    .FirstOrDefaultAsync(c => c.Id == id && c.IsDeleted);
                if (cobro is null) return ResultadoRecuperacion.NoEncontrado;
                var cotizacion = await db.Cotizaciones.IgnoreQueryFilters()
                    .Where(c => c.Id == cobro.CotizacionId && !c.IsDeleted)
                    .Select(c => new { c.SolicitudDbId })
                    .FirstOrDefaultAsync();
                if (cotizacion is null ||
                    cotizacion.SolicitudDbId.HasValue &&
                    !await db.Solicitudes.IgnoreQueryFilters().AnyAsync(s =>
                        s.Id == cotizacion.SolicitudDbId.Value && !s.IsDeleted &&
                        s.Cliente != null && !s.Cliente.IsDeleted) ||
                    cobro.OrdenTrabajoId.HasValue && !await db.OrdenesTrabajo.IgnoreQueryFilters().AnyAsync(o =>
                        o.Id == cobro.OrdenTrabajoId.Value && !o.IsDeleted))
                    return ResultadoRecuperacion.DependenciaEliminada;
                if (cobro.EstadoOrdenAntesEliminar is not null && cobro.OrdenTrabajo is not null)
                    cobro.OrdenTrabajo.Estado = cobro.EstadoOrdenAntesEliminar;
                if (cotizacion.SolicitudDbId.HasValue &&
                    (cobro.EstadoSolicitudIdAntesEliminar.HasValue || cobro.FechaFinSolicitudAntesEliminar.HasValue))
                {
                    var solicitud = await db.Solicitudes.IgnoreQueryFilters()
                        .FirstOrDefaultAsync(s => s.Id == cotizacion.SolicitudDbId.Value && !s.IsDeleted);
                    if (solicitud is null) return ResultadoRecuperacion.DependenciaEliminada;
                    if (cobro.EstadoSolicitudIdAntesEliminar.HasValue)
                        solicitud.EstadoSolicitudId = cobro.EstadoSolicitudIdAntesEliminar.Value;
                    solicitud.FechaFin = cobro.FechaFinSolicitudAntesEliminar;
                }
                cobro.EstadoOrdenAntesEliminar = null;
                cobro.EstadoSolicitudIdAntesEliminar = null;
                cobro.FechaFinSolicitudAntesEliminar = null;
                cobro.IsDeleted = false;
                break;
            }
            case TipoRegistroRecuperable.Clientes:
            {
                var cliente = await db.Clientes.IgnoreQueryFilters()
                    .FirstOrDefaultAsync(c => c.Id == id && c.IsDeleted);
                if (cliente is null) return ResultadoRecuperacion.NoEncontrado;
                cliente.IsDeleted = false;
                break;
            }
            case TipoRegistroRecuperable.Ventas:
            {
                var venta = await db.Ventas.IgnoreQueryFilters()
                    .FirstOrDefaultAsync(v => v.Id == id && v.IsDeleted);
                if (venta is null) return ResultadoRecuperacion.NoEncontrado;
                var idsArticulo = await db.DetallesVenta.IgnoreQueryFilters()
                    .Where(d => d.VentaId == id)
                    .Select(d => d.ArticuloInventarioId)
                    .Distinct()
                    .ToListAsync();
                if (idsArticulo.Count == 0) idsArticulo.Add(venta.ArticuloInventarioId);
                if (await db.ArticulosInventario.IgnoreQueryFilters().CountAsync(a =>
                        idsArticulo.Contains(a.Id) && !a.IsDeleted) != idsArticulo.Count ||
                    venta.ClienteId.HasValue && !await db.Clientes.IgnoreQueryFilters().AnyAsync(c =>
                        c.Id == venta.ClienteId.Value && !c.IsDeleted))
                    return ResultadoRecuperacion.DependenciaEliminada;
                venta.IsDeleted = false;
                break;
            }
            case TipoRegistroRecuperable.Servicios:
            {
                var servicio = await db.Servicios.IgnoreQueryFilters()
                    .FirstOrDefaultAsync(s => s.Id == id && s.IsDeleted);
                if (servicio is null) return ResultadoRecuperacion.NoEncontrado;
                servicio.IsDeleted = false;
                break;
            }
            case TipoRegistroRecuperable.Inventario:
            {
                var articulo = await db.ArticulosInventario.IgnoreQueryFilters()
                    .FirstOrDefaultAsync(a => a.Id == id && a.IsDeleted);
                if (articulo is null) return ResultadoRecuperacion.NoEncontrado;
                articulo.IsDeleted = false;
                break;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(tipo), tipo, "Tipo de recuperación desconocido.");
        }

        await db.SaveChangesAsync();
        return ResultadoRecuperacion.Recuperado;
    }
}
