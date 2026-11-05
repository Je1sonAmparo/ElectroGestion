using ElectroGestion.Context;
using ElectroGestion.Models;
using Microsoft.EntityFrameworkCore;

namespace ElectroGestion.Services;

public class SolicitudEventoService(IDbContextFactory<Contexto> factory, AppStateService appState)
{
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
