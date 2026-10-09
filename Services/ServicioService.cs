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
        try
        {
            await using var contexto = await contextFactory.CreateDbContextAsync();
            servicio.Solicitudes.Clear();
            servicio.Detalles.Clear();
            contexto.Servicios.Add(servicio);
            return await contexto.SaveChangesAsync() > 0;
        }
        catch
        {
            return false;
        }
    }

    private async Task<bool> Modificar(Servicio servicio)
    {
        try
        {
            await using var contexto = await contextFactory.CreateDbContextAsync();
            var existente = await contexto.Servicios.FirstOrDefaultAsync(s => s.Id == servicio.Id);
            if (existente == null) return false;

            existente.ServicioId = servicio.ServicioId;
            existente.Nombre = servicio.Nombre;
            existente.Categoria = servicio.Categoria;
            existente.PrecioBase = servicio.PrecioBase;
            existente.Estado = servicio.Estado;

            return await contexto.SaveChangesAsync() > 0;
        }
        catch
        {
            return false;
        }
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
        try
        {
            await using var contexto = await contextFactory.CreateDbContextAsync();
            var tieneSolicitudes = await contexto.Solicitudes.AnyAsync(s => s.ServicioId == id);
            if (tieneSolicitudes)
            {
                return false;
            }

            return await contexto.Servicios
                .Where(s => s.Id == id)
                .ExecuteDeleteAsync() > 0;
        }
        catch
        {
            return false;
        }
    }
}
