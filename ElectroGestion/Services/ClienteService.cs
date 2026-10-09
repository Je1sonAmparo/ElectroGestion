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
        await using var contexto = await contextFactory.CreateDbContextAsync();
        contexto.Clientes.Add(cliente);
        return await contexto.SaveChangesAsync() > 0;
    }

    private async Task<bool> Modificar(Cliente cliente)
    {
        await using var contexto = await contextFactory.CreateDbContextAsync();
        var existente = await contexto.Clientes
            .Include(c => c.Telefonos)
            .FirstOrDefaultAsync(c => c.Id == cliente.Id);

        if (existente == null) return false;

        contexto.Entry(existente).CurrentValues.SetValues(cliente);

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
    {
        await using var contexto = await contextFactory.CreateDbContextAsync();
        return await contexto.Clientes
            .Where(c => c.Id == id)
            .ExecuteDeleteAsync() > 0;
    }
}
