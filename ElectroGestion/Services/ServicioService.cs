using Microsoft.EntityFrameworkCore;
using ElectroGestion.Context;
using ElectroGestion.Models;
using System.Linq.Expressions;

namespace ElectroGestion.Services;

public class ServicioService(IDbContextFactory<Contexto> contextFactory) : Aplicada1.Core.IService<Servicio, int>
{
    public async Task<bool> Guardar(Servicio servicio)
    {
        if (!await Existe(servicio.Id))
        {
            return await Insertar(servicio);
        }
        else
        {
            return await Modificar(servicio);
        }
    }

    private async Task<bool> Existe(int id)
    {
        await using var contexto = await contextFactory.CreateDbContextAsync();
        return await contexto.Servicios.AnyAsync(s => s.Id == id);
    }

    private async Task<bool> Insertar(Servicio servicio)
    {
        await using var contexto = await contextFactory.CreateDbContextAsync();
        contexto.Servicios.Add(servicio);
        return await contexto.SaveChangesAsync() > 0;
    }

    private async Task<bool> Modificar(Servicio servicio)
    {
        await using var contexto = await contextFactory.CreateDbContextAsync();
        contexto.Update(servicio);
        return await contexto.SaveChangesAsync() > 0;
    }

    public async Task<Servicio?> Buscar(int id)
    {
        await using var contexto = await contextFactory.CreateDbContextAsync();
        return await contexto.Servicios
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<List<Servicio>> GetList(Expression<Func<Servicio, bool>> criterio)
    {
        await using var contexto = await contextFactory.CreateDbContextAsync();
        return await contexto.Servicios
            .Where(criterio)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<bool> Eliminar(int id)
    {
        await using var contexto = await contextFactory.CreateDbContextAsync();
        return await contexto.Servicios
            .Where(s => s.Id == id)
            .ExecuteDeleteAsync() > 0;
    }
}
