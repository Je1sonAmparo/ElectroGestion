using ElectroGestion.Context;
using ElectroGestion.Models;
using Microsoft.EntityFrameworkCore;

namespace ElectroGestion.Services;

public class InventarioService(IDbContextFactory<Contexto> factory)
{
    public async Task<List<ArticuloInventario>> ObtenerTodosAsync()
    {
        await using var db = await factory.CreateDbContextAsync();
        return await db.ArticulosInventario.AsNoTracking().OrderByDescending(a => a.Id).ToListAsync();
    }

    public async Task<bool> GuardarAsync(ArticuloInventario articulo, int? cantidadOriginal = null)
    {
        articulo.Codigo = articulo.Codigo.Trim();
        articulo.Nombre = articulo.Nombre.Trim();
        articulo.Categoria = articulo.Categoria.Trim();
        if (articulo.Codigo.Length is 0 or > 40 || articulo.Nombre.Length is 0 or > 200 ||
            articulo.Categoria.Length > 80 || articulo.Cantidad < 0 || articulo.PrecioUnidad < 0)
            return false;

        await using var db = await factory.CreateDbContextAsync();
        if (await db.ArticulosInventario.AnyAsync(a => a.Codigo == articulo.Codigo && a.Id != articulo.Id))
            return false;

        if (articulo.Id == 0)
            db.ArticulosInventario.Add(articulo);
        else
        {
            if (cantidadOriginal.HasValue && articulo.Cantidad != cantidadOriginal.Value)
            {
                try
                {
                    return await db.ArticulosInventario
                        .Where(a => a.Id == articulo.Id && a.Cantidad == cantidadOriginal.Value)
                        .ExecuteUpdateAsync(set => set.SetProperty(a => a.Codigo, articulo.Codigo)
                            .SetProperty(a => a.Nombre, articulo.Nombre)
                            .SetProperty(a => a.Categoria, articulo.Categoria)
                            .SetProperty(a => a.PrecioUnidad, articulo.PrecioUnidad)
                            .SetProperty(a => a.Cantidad, articulo.Cantidad)) == 1;
                }
                catch (System.Data.Common.DbException) { return false; }
            }
            var existente = await db.ArticulosInventario.FindAsync(articulo.Id);
            if (existente is null) return false;
            existente.Codigo = articulo.Codigo;
            existente.Nombre = articulo.Nombre;
            existente.Categoria = articulo.Categoria;
            if (!cantidadOriginal.HasValue) existente.Cantidad = articulo.Cantidad;
            existente.PrecioUnidad = articulo.PrecioUnidad;
        }

        try
        {
            await db.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateException)
        {
            return false;
        }
    }

    public async Task<bool> AgregarStockAsync(int id, int cantidad)
    {
        if (cantidad <= 0) return false;
        await using var db = await factory.CreateDbContextAsync();
        return await db.ArticulosInventario.Where(a => a.Id == id && a.Cantidad <= int.MaxValue - cantidad)
            .ExecuteUpdateAsync(set => set.SetProperty(a => a.Cantidad, a => a.Cantidad + cantidad)) == 1;
    }
}
