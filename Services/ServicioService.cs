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
        => await EliminarVarios([id]) == 1;

    public async Task<HashSet<int>> ObtenerElegiblesAsync(IEnumerable<int> ids)
    {
        var listaIds = ids.Distinct().ToList();
        if (listaIds.Count == 0) return [];

        await using var contexto = await contextFactory.CreateDbContextAsync();
        var vinculados = await contexto.Solicitudes
            .IgnoreQueryFilters()
            .Where(s => listaIds.Contains(s.ServicioId))
            .Select(s => s.ServicioId)
            .ToListAsync();
        var vinculadosDetalles = await contexto.DetallesSolicitud
            .Where(d => listaIds.Contains(d.ServicioId))
            .Select(d => d.ServicioId)
            .ToListAsync();

        return listaIds.Except(vinculados).Except(vinculadosDetalles).ToHashSet();
    }

    public async Task<int> EliminarVarios(IReadOnlyCollection<int> ids)
    {
        if (ids.Count == 0 || ids.Count != ids.Distinct().Count()) return 0;

        try
        {
            await using var contexto = await contextFactory.CreateDbContextAsync();
            await using var transaccion = await contexto.Database.BeginTransactionAsync();

            var vinculados = await contexto.Solicitudes
                .IgnoreQueryFilters()
                .Where(s => ids.Contains(s.ServicioId))
                .Select(s => s.ServicioId)
                .ToListAsync();
            var vinculadosDetalles = await contexto.DetallesSolicitud
                .Where(d => ids.Contains(d.ServicioId))
                .Select(d => d.ServicioId)
                .ToListAsync();
            var elegibles = ids.Except(vinculados).Except(vinculadosDetalles).ToList();
            if (elegibles.Count == 0)
            {
                await transaccion.RollbackAsync();
                return 0;
            }

            await contexto.Servicios
                .Where(s => elegibles.Contains(s.Id))
                .ExecuteUpdateAsync(set => set.SetProperty(s => s.IsDeleted, true));

            await transaccion.CommitAsync();
            return elegibles.Count;
        }
        catch
        {
            return 0;
        }
    }
}
