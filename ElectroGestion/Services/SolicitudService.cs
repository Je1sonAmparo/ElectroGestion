using Microsoft.EntityFrameworkCore;
using ElectroGestion.Context;
using ElectroGestion.Models;
using System.Linq.Expressions;

namespace ElectroGestion.Services;

public class SolicitudService(IDbContextFactory<Contexto> contextFactory) : Aplicada1.Core.IService<Solicitud, int>
{
    public async Task<bool> Guardar(Solicitud solicitud)
    {
        if (!await Existe(solicitud.Id))
        {
            return await Insertar(solicitud);
        }
        else
        {
            return await Modificar(solicitud);
        }
    }

    private async Task<bool> Existe(int id)
    {
        await using var contexto = await contextFactory.CreateDbContextAsync();
        return await contexto.Solicitudes.AnyAsync(s => s.Id == id);
    }

    private async Task<bool> Insertar(Solicitud solicitud)
    {
        await using var contexto = await contextFactory.CreateDbContextAsync();
        contexto.Solicitudes.Add(solicitud);
        return await contexto.SaveChangesAsync() > 0;
    }

    private async Task<bool> Modificar(Solicitud solicitud)
    {
        await using var contexto = await contextFactory.CreateDbContextAsync();
        contexto.Update(solicitud);
        return await contexto.SaveChangesAsync() > 0;
    }

    public async Task<Solicitud?> Buscar(int id)
    {
        await using var contexto = await contextFactory.CreateDbContextAsync();
        return await contexto.Solicitudes
            .Include(s => s.Cliente)
            .Include(s => s.Servicio)
            .Include(s => s.EstadoSolicitud)
            .Include(s => s.Detalles)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<List<Solicitud>> GetList(Expression<Func<Solicitud, bool>> criterio)
    {
        await using var contexto = await contextFactory.CreateDbContextAsync();
        return await contexto.Solicitudes
            .Include(s => s.Cliente)
            .Include(s => s.Servicio)
            .Include(s => s.EstadoSolicitud)
            .Include(s => s.Detalles)
            .Where(criterio)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<bool> Eliminar(int id)
    {
        await using var contexto = await contextFactory.CreateDbContextAsync();
        return await contexto.Solicitudes
            .Where(s => s.Id == id)
            .ExecuteDeleteAsync() > 0;
    }
}
