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
        articulo.CodigoBarra = string.IsNullOrWhiteSpace(articulo.CodigoBarra) ? null : articulo.CodigoBarra.Trim();
        articulo.Nombre = articulo.Nombre.Trim();
        articulo.Categoria = articulo.Categoria.Trim();
        if (articulo.Codigo.Length is 0 or > 40 || articulo.Nombre.Length is 0 or > 200 ||
            articulo.Categoria.Length > 80 || articulo.CodigoBarra?.Length > 80 ||
            articulo.Cantidad < 0 || articulo.PrecioUnidad < 0)
            return false;

        await using var db = await factory.CreateDbContextAsync();
        if (await db.ArticulosInventario.AnyAsync(a => a.Codigo == articulo.Codigo && a.Id != articulo.Id))
            return false;
        if (articulo.CodigoBarra is not null &&
            await db.ArticulosInventario.AnyAsync(a => a.CodigoBarra == articulo.CodigoBarra && a.Id != articulo.Id))
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
                            .SetProperty(a => a.CodigoBarra, articulo.CodigoBarra)
                            .SetProperty(a => a.Nombre, articulo.Nombre)
                            .SetProperty(a => a.Categoria, articulo.Categoria)
                            .SetProperty(a => a.PrecioUnidad, articulo.PrecioUnidad)
                            .SetProperty(a => a.Cantidad, articulo.Cantidad)) == 1;
                }
                catch (System.Data.Common.DbException) { return false; }
            }
            var existente = await db.ArticulosInventario.FirstOrDefaultAsync(a => a.Id == articulo.Id);
            if (existente is null) return false;
            existente.Codigo = articulo.Codigo;
            existente.CodigoBarra = articulo.CodigoBarra;
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

    // Un artículo es elegible para eliminar cuando no hay registros que lo referencien
    // (FKs reales: Venta.ArticuloInventarioId y DetalleVenta.ArticuloInventarioId; además, los
    // detalles de cotización guardan el Id como instantánea histórica).
    private static async Task<bool> ArticuloReferenciadoAsync(Contexto db, int id) =>
        await db.Ventas.IgnoreQueryFilters().AnyAsync(v => v.ArticuloInventarioId == id) ||
        await db.DetallesVenta.IgnoreQueryFilters().AnyAsync(d => d.ArticuloInventarioId == id) ||
        await db.DetallesCotizacion.IgnoreQueryFilters().AnyAsync(d => d.ArticuloInventarioId == id);

    public async Task<HashSet<int>> ObtenerElegiblesAsync(IEnumerable<int> ids)
    {
        var idList = ids.Distinct().Where(i => i > 0).ToList();
        if (idList.Count == 0) return [];
        await using var db = await factory.CreateDbContextAsync();
        var referenciados = new HashSet<int>();
        var enVentas = await db.Ventas.IgnoreQueryFilters().AsNoTracking()
            .Where(v => idList.Contains(v.ArticuloInventarioId))
            .Select(v => v.ArticuloInventarioId).ToListAsync();
        foreach (var id in enVentas) referenciados.Add(id);
        var enDetallesVenta = await db.DetallesVenta.IgnoreQueryFilters().AsNoTracking()
            .Where(d => idList.Contains(d.ArticuloInventarioId))
            .Select(d => d.ArticuloInventarioId).ToListAsync();
        foreach (var id in enDetallesVenta) referenciados.Add(id);
        var enDetallesCotizacion = await db.DetallesCotizacion.IgnoreQueryFilters().AsNoTracking()
            .Where(d => d.ArticuloInventarioId > 0 && idList.Contains(d.ArticuloInventarioId))
            .Select(d => d.ArticuloInventarioId).ToListAsync();
        foreach (var id in enDetallesCotizacion) referenciados.Add(id);
        return idList.Where(i => !referenciados.Contains(i)).ToHashSet();
    }

    public async Task<(int eliminados, int omitidos)> EliminarVariosAsync(IReadOnlyCollection<int> ids)
    {
        var idList = ids.Distinct().Where(i => i > 0).ToList();
        if (idList.Count == 0) return (0, 0);
        await using var db = await factory.CreateDbContextAsync();
        await using var transaccion = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
        try
        {
            var articulos = await db.ArticulosInventario.Where(a => idList.Contains(a.Id)).ToListAsync();
            var elegibles = new List<ArticuloInventario>();
            foreach (var articulo in articulos)
            {
                if (!await ArticuloReferenciadoAsync(db, articulo.Id)) elegibles.Add(articulo);
                else db.Entry(articulo).State = EntityState.Detached;
            }
            if (elegibles.Count > 0)
            {
                foreach (var articulo in elegibles) articulo.IsDeleted = true;
                await db.SaveChangesAsync();
            }
            await transaccion.CommitAsync();
            return (elegibles.Count, articulos.Count - elegibles.Count);
        }
        catch (Exception ex) when (ex is System.Data.Common.DbException or DbUpdateException)
        {
            await transaccion.RollbackAsync();
            return (0, idList.Count);
        }
    }
}
