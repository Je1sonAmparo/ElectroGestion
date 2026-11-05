using Microsoft.EntityFrameworkCore;
using ElectroGestion.Context;
using ElectroGestion.Models;
using System.Linq.Expressions;

namespace ElectroGestion.Services;

public class ClienteService(IDbContextFactory<Contexto> contextFactory) : Aplicada1.Core.IService<Cliente, int>
{
    public async Task<bool> Guardar(Cliente cliente)
    {
        if (!await Existe(cliente.Id))
        {
            return await Insertar(cliente);
        }
        else
        {
            return await Modificar(cliente);
        }
    }

    private async Task<bool> Existe(int id)
    {
        await using var contexto = await contextFactory.CreateDbContextAsync();
        return await contexto.Clientes.AnyAsync(c => c.Id == id);
    }

    private async Task<bool> Insertar(Cliente cliente)
    {
        try
        {
            await using var contexto = await contextFactory.CreateDbContextAsync();
            contexto.Clientes.Add(cliente);
            return await contexto.SaveChangesAsync() > 0;
        }
        catch
        {
            return false;
        }
    }

    private async Task<bool> Modificar(Cliente cliente)
    {
        try
        {
            await using var contexto = await contextFactory.CreateDbContextAsync();
            var existente = await contexto.Clientes
                .Include(c => c.Telefonos)
                .FirstOrDefaultAsync(c => c.Id == cliente.Id);

            if (existente == null) return false;

            existente.Nombre = cliente.Nombre;
            existente.Correo = cliente.Correo;
            existente.Rnc = cliente.Rnc;
            existente.Direccion = cliente.Direccion;
            if (cliente.Servicios > 0)
                existente.Servicios = cliente.Servicios;
            if (!string.IsNullOrWhiteSpace(cliente.Ultimo))
                existente.Ultimo = cliente.Ultimo;

            contexto.ClientesTelefonos.RemoveRange(existente.Telefonos);
            foreach (var t in cliente.Telefonos)
            {
                existente.Telefonos.Add(new ClienteTelefono
                {
                    ClienteId = cliente.Id,
                    Numero = t.Numero,
                    Tipo = t.Tipo
                });
            }

            return await contexto.SaveChangesAsync() > 0;
        }
        catch
        {
            return false;
        }
    }

    public async Task<Cliente?> Buscar(int id)
    {
        await using var contexto = await contextFactory.CreateDbContextAsync();
        return await contexto.Clientes
            .Include(c => c.Telefonos)
            .Include(c => c.Solicitudes)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<List<Cliente>> GetList(Expression<Func<Cliente, bool>> criterio)
    {
        await using var contexto = await contextFactory.CreateDbContextAsync();
        return await contexto.Clientes
            .Include(c => c.Telefonos)
            .Include(c => c.Solicitudes)
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
        var protegidos = await contexto.Solicitudes
            .IgnoreQueryFilters()
            .Where(s => listaIds.Contains(s.ClienteId) && (s.EstadoSolicitudId == 2 || s.EstadoSolicitudId == 5))
            .Select(s => s.ClienteId)
            .ToListAsync();

        return listaIds.Except(protegidos).ToHashSet();
    }

    public async Task<int> EliminarVarios(IReadOnlyCollection<int> ids)
    {
        if (ids.Count == 0 || ids.Count != ids.Distinct().Count()) return 0;

        try
        {
            await using var contexto = await contextFactory.CreateDbContextAsync();
            await using var transaccion = await contexto.Database.BeginTransactionAsync();

            var protegidos = await contexto.Solicitudes
                .IgnoreQueryFilters()
                .Where(s => ids.Contains(s.ClienteId) && (s.EstadoSolicitudId == 2 || s.EstadoSolicitudId == 5))
                .Select(s => s.ClienteId)
                .ToListAsync();
            var elegibles = ids.Except(protegidos).ToList();
            if (elegibles.Count == 0)
            {
                await transaccion.RollbackAsync();
                return 0;
            }

            var solicitudIds = await contexto.Solicitudes
                .IgnoreQueryFilters()
                .Where(s => elegibles.Contains(s.ClienteId))
                .Select(s => s.Id)
                .ToListAsync();
            if (solicitudIds.Count > 0)
            {
                await contexto.Visitas
                    .IgnoreQueryFilters()
                    .Where(v => solicitudIds.Contains(v.SolicitudId))
                    .ExecuteDeleteAsync();
                await contexto.Solicitudes
                    .IgnoreQueryFilters()
                    .Where(s => solicitudIds.Contains(s.Id))
                    .ExecuteDeleteAsync();
            }

            await contexto.ClientesTelefonos
                .Where(t => elegibles.Contains(t.ClienteId))
                .ExecuteDeleteAsync();
            var eliminados = await contexto.Clientes
                .Where(c => elegibles.Contains(c.Id))
                .ExecuteDeleteAsync();

            await transaccion.CommitAsync();
            return elegibles.Count;
        }
        catch
        {
            return 0;
        }
    }
}
