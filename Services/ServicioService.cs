using Microsoft.EntityFrameworkCore;
using ElectroGestion.Context;
using ElectroGestion.Models;
using System.Linq.Expressions;

namespace ElectroGestion.Services;

public class ServicioService(IDbContextFactory<Contexto> contextFactory) : Aplicada1.Core.IService<Servicio, int>
{
    public async Task<bool> Guardar(Servicio servicio)
    {
        servicio.ServicioId = servicio.ServicioId.Trim();
        servicio.Nombre = servicio.Nombre.Trim();
        servicio.Categoria = servicio.Categoria.Trim();
        if (servicio.ServicioId.Length is 0 or > 20 || servicio.Nombre.Length is 0 or > 150 ||
            servicio.Categoria.Length > 80 || servicio.Estado is not ("Activo" or "Inactivo"))
            return false;

        return servicio.Id == 0 ? await Insertar(servicio) : await Modificar(servicio);
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
            existente.Estado = servicio.Estado;

            await contexto.SaveChangesAsync();
            return true;
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
            if (await contexto.Solicitudes.IgnoreQueryFilters().AnyAsync(s => s.ServicioId == id) ||
                await contexto.DetallesSolicitud.AnyAsync(d => d.ServicioId == id))
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
