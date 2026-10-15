using ElectroGestion.Context;
using ElectroGestion.Models;
using Microsoft.EntityFrameworkCore;

namespace ElectroGestion.Services;

public class VisitaService(IDbContextFactory<Contexto> contextFactory)
{
    public async Task<List<Visita>> ObtenerTodasAsync()
    {
        await using var contexto = await contextFactory.CreateDbContextAsync();
        return await contexto.Visitas
            .Include(v => v.Solicitud!)
                .ThenInclude(s => s.Cliente)
            .Include(v => v.Solicitud!)
                .ThenInclude(s => s.Servicio)
            .Include(v => v.Solicitud!)
                .ThenInclude(s => s.EstadoSolicitud)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<bool> GuardarAsync(Visita visita)
    {
        await using var contexto = await contextFactory.CreateDbContextAsync();
        if (visita.Id == 0)
        {
            visita.Solicitud = null;
            contexto.Visitas.Add(visita);
        }
        else
        {
            var existente = await contexto.Visitas.FirstOrDefaultAsync(v => v.Id == visita.Id);
            if (existente is null)
            {
                return false;
            }

            existente.SolicitudId = visita.SolicitudId;
            existente.Fecha = visita.Fecha;
            existente.Hora = visita.Hora;
            existente.Estado = visita.Estado;
        }

        return await contexto.SaveChangesAsync() > 0;
    }

    public async Task<bool> EliminarAsync(int id)
    {
        await using var contexto = await contextFactory.CreateDbContextAsync();
        return await contexto.Visitas
            .Where(v => v.Id == id)
            .ExecuteDeleteAsync() > 0;
    }

    public async Task<bool> CambiarEstadoAsync(int id, string estado)
    {
        await using var contexto = await contextFactory.CreateDbContextAsync();
        var visita = await contexto.Visitas.FirstOrDefaultAsync(v => v.Id == id);
        if (visita is null)
        {
            return false;
        }

        visita.Estado = estado;
        return await contexto.SaveChangesAsync() > 0;
    }
}
