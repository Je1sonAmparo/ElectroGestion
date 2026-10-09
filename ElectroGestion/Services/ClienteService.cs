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
    {
        try
        {
            await using var contexto = await contextFactory.CreateDbContextAsync();
            var tieneSolicitudes = await contexto.Solicitudes.AnyAsync(s => s.ClienteId == id);
            if (tieneSolicitudes)
            {
                return false;
            }

            await contexto.ClientesTelefonos.Where(t => t.ClienteId == id).ExecuteDeleteAsync();
            return await contexto.Clientes
                .Where(c => c.Id == id)
                .ExecuteDeleteAsync() > 0;
        }
        catch
        {
            return false;
        }
    }
}
