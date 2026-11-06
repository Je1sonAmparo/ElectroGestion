using ElectroGestion.Context;
using ElectroGestion.Models;
using Microsoft.EntityFrameworkCore;

namespace ElectroGestion.Services;

public class SolicitudEventoService(IDbContextFactory<Contexto> factory, AppStateService appState)
{
    public async Task<Dictionary<int, DateTime>> ObtenerUltimasFechasActividadAsync(IEnumerable<int> solicitudIds)
    {
        if (!appState.IsAuthenticated) return [];
        var ids = solicitudIds.Distinct().ToArray();
        if (ids.Length == 0) return [];
        await using var db = await factory.CreateDbContextAsync();
        var fechas = await db.SolicitudesEventos.AsNoTracking()
            .Where(e => ids.Contains(e.SolicitudId))
            .GroupBy(e => e.SolicitudId)
            .Select(g => new { SolicitudId = g.Key, Fecha = g.Max(e => e.Fecha) })
            .ToListAsync();
        return fechas.ToDictionary(x => x.SolicitudId, x => x.Fecha);
    }

    public async Task<List<SolicitudEvento>> ObtenerAsync(int solicitudId)
    {
        if (!appState.IsAuthenticated) return [];
        await using var db = await factory.CreateDbContextAsync();
        return await db.SolicitudesEventos.AsNoTracking()
            .Where(e => e.SolicitudId == solicitudId)
            .OrderByDescending(e => e.Fecha)
            .ToListAsync();
    }
}
