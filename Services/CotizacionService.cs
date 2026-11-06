using ElectroGestion.Context;
using ElectroGestion.Models;
using Microsoft.EntityFrameworkCore;

namespace ElectroGestion.Services;

public enum ResultadoEliminarCotizacion { Eliminada, VinculadaAOrden, VinculadaACobro, SolicitudFinalizada, NoEncontrada }

public class CotizacionService(IDbContextFactory<Contexto> contextFactory)
{
    public const string EnRevision = "En revisión";
    public const decimal TasaItbis = 18m;
    public static readonly string[] Estados = [EnRevision, "Enviada al cliente", "Aprobada", "Rechazada"];
    private const string EnEsperaAdministracion = "En Espera Administración";

    public async Task<List<Cotizacion>> ObtenerTodasAsync()
    {
        await using var db = await contextFactory.CreateDbContextAsync();
        return await db.Cotizaciones.AsNoTracking()
            .Include(c => c.Solicitud).ThenInclude(s => s!.Cliente).ThenInclude(c => c!.Telefonos)
            .Include(c => c.Solicitud).ThenInclude(s => s!.Servicio)
            .Include(c => c.Solicitud).ThenInclude(s => s!.Levantamiento)
            .Include(c => c.Detalles)
            .OrderBy(c => c.Estado == EnRevision ? 0 : c.Estado == "Enviada al cliente" ? 1 : c.Estado == "Rechazada" ? 2 : c.Estado == "Aprobada" ? 3 : 4)
            .ThenBy(c => c.Fecha)
            .ThenBy(c => c.Id).ToListAsync();
    }

    public async Task<List<Solicitud>> ObtenerLevantamientosPendientesAsync()
    {
        await using var db = await contextFactory.CreateDbContextAsync();
        return await db.Solicitudes.AsNoTracking()
            .Where(s => s.EstadoSolicitud != null &&
                (s.EstadoSolicitud.Nombre == EnEsperaAdministracion ||
                    db.Visitas.Any(v => v.SolicitudId == s.Id && !v.IsDeleted && v.Estado == EnEsperaAdministracion)) &&
                s.Levantamiento != null &&
                !db.Cotizaciones.Any(c => c.SolicitudDbId == s.Id))
            .Include(s => s.Cliente).Include(s => s.Servicio).Include(s => s.Levantamiento)
            .OrderBy(s => s.FechaSolicitud).ToListAsync();
    }

    public async Task<bool> CrearAsync(int solicitudId, IReadOnlyCollection<DetalleCotizacion> detalles, decimal montoManoObra)
    {
        if (!DetallesValidos(detalles, montoManoObra)) return false;
        await using var db = await contextFactory.CreateDbContextAsync();
        var solicitud = await db.Solicitudes.Include(s => s.Cliente)
            .FirstOrDefaultAsync(s => s.Id == solicitudId && s.Levantamiento != null && s.EstadoSolicitud != null &&
                (s.EstadoSolicitud.Nombre == EnEsperaAdministracion ||
                    db.Visitas.Any(v => v.SolicitudId == s.Id && !v.IsDeleted && v.Estado == EnEsperaAdministracion)));
        if (solicitud is null || await db.Cotizaciones.AnyAsync(c => c.SolicitudDbId == solicitudId))
            return false;

        db.Cotizaciones.Add(new Cotizacion
        {
            SolicitudDbId = solicitud.Id,
            SolicitudId = $"SOL-{solicitud.Id:D4}",
            Cliente = solicitud.Cliente?.Nombre ?? "",
            Fecha = DateTime.Now,
            MontoTotal = CalcularTotal(detalles, montoManoObra),
            PorcentajeItbis = TasaItbis,
            MontoManoObra = montoManoObra,
            Estado = EnRevision,
            Detalles = detalles.Select(CopiarDetalle).ToList()
        });
        await db.SaveChangesAsync();
        var estadoRevision = await db.EstadosSolicitud.FirstAsync(e => e.Nombre == EnRevision);
        solicitud.EstadoSolicitudId = estadoRevision.Id;
        RegistroEventosSolicitud.Agregar(db, solicitudId, "Cotización creada", "Se generó una cotización en estado En revisión.");
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ActualizarDetallesAsync(int id, IReadOnlyCollection<DetalleCotizacion> detalles, decimal montoManoObra)
    {
        if (!DetallesValidos(detalles, montoManoObra)) return false;
        await using var db = await contextFactory.CreateDbContextAsync();
        var cotizacion = await db.Cotizaciones.Include(c => c.Detalles).FirstOrDefaultAsync(c => c.Id == id);
        if (cotizacion is null || cotizacion.Estado != EnRevision ||
            (cotizacion.SolicitudDbId.HasValue && await SolicitudFinalizadaAsync(db, cotizacion.SolicitudDbId.Value))) return false;
        db.DetallesCotizacion.RemoveRange(cotizacion.Detalles);
        cotizacion.Detalles = detalles.Select(CopiarDetalle).ToList();
        cotizacion.PorcentajeItbis = TasaItbis;
        cotizacion.MontoManoObra = montoManoObra;
        cotizacion.MontoTotal = CalcularTotal(detalles, montoManoObra);
        var ordenVinculada = await db.OrdenesTrabajo.FirstOrDefaultAsync(o => o.CotizacionId == id);
        if (ordenVinculada is not null)
            ordenVinculada.MaterialesCotizados = detalles.Select(d => new MaterialOrden
            {
                Codigo = d.CodigoArticulo, Nombre = d.NombreArticulo, Cantidad = d.Cantidad
            }).ToList();
        if (cotizacion.SolicitudDbId.HasValue)
            RegistroEventosSolicitud.Agregar(db, cotizacion.SolicitudDbId.Value, "Cotización actualizada", "Se actualizaron materiales, mano de obra o importes.");
        await db.SaveChangesAsync();
        return true;
    }

    private static bool DetallesValidos(IReadOnlyCollection<DetalleCotizacion> detalles, decimal montoManoObra) =>
        montoManoObra >= 0 && detalles.All(d => d.ArticuloInventarioId > 0 && d.Cantidad > 0 &&
            d.PrecioUnitario >= 0 && !string.IsNullOrWhiteSpace(d.CodigoArticulo) &&
            !string.IsNullOrWhiteSpace(d.NombreArticulo)) && detalles.Sum(d => d.Subtotal) + montoManoObra > 0;

    public static decimal CalcularImpuesto(decimal subtotal, decimal porcentaje) => Math.Round(subtotal * porcentaje / 100m, 2, MidpointRounding.AwayFromZero);
    public static decimal CalcularTotal(IReadOnlyCollection<DetalleCotizacion> detalles, decimal montoManoObra)
    {
        var subtotal = detalles.Sum(d => d.Subtotal) + montoManoObra;
        return subtotal + CalcularImpuesto(subtotal, TasaItbis);
    }

    private static DetalleCotizacion CopiarDetalle(DetalleCotizacion detalle) => new()
    {
        ArticuloInventarioId = detalle.ArticuloInventarioId,
        CodigoArticulo = detalle.CodigoArticulo,
        NombreArticulo = detalle.NombreArticulo,
        Cantidad = detalle.Cantidad,
        PrecioUnitario = detalle.PrecioUnitario
    };

    public async Task<bool> CambiarEstadoAsync(int id, string nuevoEstado, string motivoRechazo = "")
    {
        if (!Estados.Contains(nuevoEstado)) return false;
        await using var db = await contextFactory.CreateDbContextAsync();
        var cotizacion = await db.Cotizaciones.FirstOrDefaultAsync(c => c.Id == id);
        if (cotizacion is null) return false;
        if (cotizacion.SolicitudDbId.HasValue && await SolicitudFinalizadaAsync(db, cotizacion.SolicitudDbId.Value)) return false;

        if (cotizacion.Estado == EnRevision && nuevoEstado == "Enviada al cliente")
        {
            cotizacion.FechaEnvio = DateTime.Now;
        }
        else if (cotizacion.Estado == "Enviada al cliente" && nuevoEstado is "Aprobada" or "Rechazada")
        {
            cotizacion.FechaRespuesta = DateTime.Now;
            cotizacion.MotivoRechazo = nuevoEstado == "Rechazada" ? motivoRechazo.Trim() : "";
        }
        else if (cotizacion.Estado == "Enviada al cliente" && nuevoEstado == EnRevision)
        {
            cotizacion.FechaEnvio = null;
            cotizacion.FechaRespuesta = null;
            cotizacion.MotivoRechazo = "";
        }
        else if (cotizacion.Estado == "Rechazada" && nuevoEstado == EnRevision)
        {
            cotizacion.FechaEnvio = null;
            cotizacion.FechaRespuesta = null;
            cotizacion.MotivoRechazo = "";
        }
        else if (cotizacion.Estado is "Aprobada" or "Rechazada" && nuevoEstado == "Enviada al cliente")
        {
            cotizacion.FechaRespuesta = null;
            cotizacion.MotivoRechazo = "";
        }
        else
        {
            return false;
        }

        cotizacion.Estado = nuevoEstado;
        if (cotizacion.SolicitudDbId.HasValue)
        {
            var estadoSolicitud = await db.EstadosSolicitud.FirstOrDefaultAsync(e => e.Nombre == nuevoEstado);
            var solicitud = await db.Solicitudes.FirstOrDefaultAsync(s => s.Id == cotizacion.SolicitudDbId.Value);
            if (estadoSolicitud is not null && solicitud is not null) solicitud.EstadoSolicitudId = estadoSolicitud.Id;
            RegistroEventosSolicitud.Agregar(db, cotizacion.SolicitudDbId.Value, "Estado de cotización actualizado", $"La cotización pasó a {nuevoEstado}.");
        }
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<ResultadoEliminarCotizacion> EliminarAsync(int id)
    {
        await using var db = await contextFactory.CreateDbContextAsync();
        var cotizacion = await db.Cotizaciones.FirstOrDefaultAsync(c => c.Id == id);
        if (cotizacion is null) return ResultadoEliminarCotizacion.NoEncontrada;
        var resultado = await EvaluarGuardasAsync(db, cotizacion);
        if (resultado != ResultadoEliminarCotizacion.Eliminada) return resultado;

        if (cotizacion.SolicitudDbId.HasValue)
            RegistroEventosSolicitud.Agregar(db, cotizacion.SolicitudDbId.Value, "Cotización eliminada", "Se eliminó la cotización vinculada.");
        cotizacion.IsDeleted = true;
        await db.SaveChangesAsync();
        return ResultadoEliminarCotizacion.Eliminada;
    }

    public async Task<int> EliminarVariosAsync(IReadOnlyCollection<int> ids)
    {
        if (ids.Count == 0 || ids.Count != ids.Distinct().Count()) return 0;
        await using var db = await contextFactory.CreateDbContextAsync();
        await using var transaccion = await db.Database.BeginTransactionAsync();
        var cotizaciones = await db.Cotizaciones.Where(c => ids.Contains(c.Id)).ToListAsync();
        var elegibles = new List<Cotizacion>();
        foreach (var cotizacion in cotizaciones)
        {
            if (await EvaluarGuardasAsync(db, cotizacion) == ResultadoEliminarCotizacion.Eliminada)
                elegibles.Add(cotizacion);
        }

        foreach (var cotizacion in elegibles)
        {
            if (cotizacion.SolicitudDbId.HasValue)
                RegistroEventosSolicitud.Agregar(db, cotizacion.SolicitudDbId.Value, "Cotización eliminada", "Se eliminó la cotización vinculada.");
            cotizacion.IsDeleted = true;
        }
        await db.SaveChangesAsync();
        await transaccion.CommitAsync();
        return elegibles.Count;
    }

    public async Task<HashSet<int>> ObtenerElegiblesAsync(IEnumerable<int> ids)
    {
        var idsSolicitud = ids.Distinct().ToList();
        if (idsSolicitud.Count == 0) return [];
        await using var db = await contextFactory.CreateDbContextAsync();
        var cotizaciones = await db.Cotizaciones.AsNoTracking().Where(c => idsSolicitud.Contains(c.Id)).ToListAsync();
        var elegibles = new HashSet<int>();
        foreach (var cotizacion in cotizaciones)
        {
            if (await EvaluarGuardasAsync(db, cotizacion) == ResultadoEliminarCotizacion.Eliminada)
                elegibles.Add(cotizacion.Id);
        }
        return elegibles;
    }

    private static async Task<ResultadoEliminarCotizacion> EvaluarGuardasAsync(Contexto db, Cotizacion cotizacion)
    {
        if (cotizacion.SolicitudDbId.HasValue && await SolicitudFinalizadaAsync(db, cotizacion.SolicitudDbId.Value))
            return ResultadoEliminarCotizacion.SolicitudFinalizada;
        if (await db.OrdenesTrabajo.IgnoreQueryFilters().AnyAsync(o => o.CotizacionId == cotizacion.Id))
            return ResultadoEliminarCotizacion.VinculadaAOrden;
        if (await db.Cobros.IgnoreQueryFilters().AnyAsync(c => c.CotizacionId == cotizacion.Id))
            return ResultadoEliminarCotizacion.VinculadaACobro;
        return ResultadoEliminarCotizacion.Eliminada;
    }

    private static Task<bool> SolicitudFinalizadaAsync(Contexto db, int solicitudId) =>
        db.Solicitudes.Where(s => s.Id == solicitudId)
            .AnyAsync(s => s.EstadoSolicitud != null && s.EstadoSolicitud.Nombre == "Finalizada");
}
