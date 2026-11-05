using ElectroGestion.Context;
using ElectroGestion.Models;
using Microsoft.EntityFrameworkCore;

namespace ElectroGestion.Services;

public sealed record InicioDashboardDatos(
    IReadOnlyList<Visita> VisitasHoy,
    IReadOnlyList<Solicitud> ActividadReciente,
    int SolicitudesPendientes,
    int LevantamientosPendientes,
    int OrdenesPendientes);

public class InicioDashboardService(IDbContextFactory<Contexto> contextFactory, AppStateService appState)
{
    public async Task<InicioDashboardDatos> ObtenerDatosAsync(DateTime fecha)
    {
        if (!appState.IsAuthenticated)
            return new([], [], 0, 0, 0);

        await using var db = await contextFactory.CreateDbContextAsync();
        var esTecnico = appState.Session?.Rol == "Técnico";
        var tecnicoId = appState.Session?.Id;

        var visitasQuery = db.Visitas.AsNoTracking()
            .Where(v => !v.IsDeleted && v.Solicitud != null && !v.Solicitud.IsDeleted);
        if (esTecnico && tecnicoId.HasValue)
            visitasQuery = visitasQuery.Where(v => v.TecnicoId == tecnicoId.Value);
        else if (esTecnico)
            visitasQuery = visitasQuery.Where(_ => false);

        var visitasHoy = await visitasQuery
            .Where(v => v.Fecha.Date == fecha.Date && v.Estado != "Cancelada" && v.Estado != "Finalizada")
            .Include(v => v.Solicitud).ThenInclude(s => s!.Cliente)
            .Include(v => v.Solicitud).ThenInclude(s => s!.Servicio)
            .OrderBy(v => v.Hora)
            .ToListAsync();

        var solicitudesQuery = db.Solicitudes.AsNoTracking();
        if (esTecnico && tecnicoId.HasValue)
        {
            solicitudesQuery = solicitudesQuery.Where(s => db.Visitas.Any(v =>
                v.SolicitudId == s.Id && v.TecnicoId == tecnicoId.Value && !v.IsDeleted && v.Estado != "Cancelada"));
        }
        else if (esTecnico)
        {
            solicitudesQuery = solicitudesQuery.Where(_ => false);
        }

        var solicitudesPendientes = await solicitudesQuery
            .CountAsync(s => s.EstadoSolicitud != null && s.EstadoSolicitud.Nombre == "Pendiente");
        var levantamientosPendientes = await solicitudesQuery
            .CountAsync(s => s.EstadoSolicitud != null && s.EstadoSolicitud.Nombre == "Pendiente" &&
                db.Visitas.Any(v => v.SolicitudId == s.Id && !v.IsDeleted && v.Estado != "Cancelada"));

        var ordenesQuery = db.OrdenesTrabajo.AsNoTracking().Where(o => o.Estado == "En proceso");
        if (esTecnico && tecnicoId.HasValue)
        {
            ordenesQuery = ordenesQuery.Where(o => db.Visitas.Any(v =>
                v.SolicitudId == o.SolicitudId && v.TecnicoId == tecnicoId.Value && !v.IsDeleted && v.Estado != "Cancelada"));
        }
        else if (esTecnico)
        {
            ordenesQuery = ordenesQuery.Where(_ => false);
        }
        var ordenesPendientes = await ordenesQuery.CountAsync();

        var actividadReciente = await solicitudesQuery
            .Include(s => s.Cliente)
            .Include(s => s.Servicio)
            .OrderByDescending(s => s.FechaSolicitud)
            .ThenByDescending(s => s.Id)
            .Take(3)
            .ToListAsync();

        return new(visitasHoy, actividadReciente, solicitudesPendientes, levantamientosPendientes, ordenesPendientes);
    }
}
