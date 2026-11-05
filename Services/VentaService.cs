using System.Data;
using System.Data.Common;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using ElectroGestion.Context;
using ElectroGestion.Models;
using Microsoft.EntityFrameworkCore;

namespace ElectroGestion.Services;

public sealed record MesVentas(DateTime Mes, int Ventas, long Unidades, decimal Ingresos, decimal Itbis);

public class VentaService(IDbContextFactory<Contexto> factory, AppStateService appState)
{
    private static DateTime AhoraDominicana => DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(-4)).DateTime;

    public async Task<List<Venta>> ListarAsync()
    {
        if (!appState.EsGerente) return [];
        await using var db = await factory.CreateDbContextAsync();
        return await db.Ventas.AsNoTracking().Include(v => v.Detalles).OrderByDescending(v => v.Id).ToListAsync();
    }

    public async Task<string?> GuardarAsync(Venta datos, bool registrar)
    {
        if (!appState.EsGerente) return "Acceso restringido.";
        if (datos.Detalles.Count == 0 || datos.Detalles.Any(d => d.Cantidad <= 0) ||
            datos.Detalles.Select(d => d.ArticuloInventarioId).Distinct().Count() != datos.Detalles.Count ||
            datos.TipoComprobante is not (Venta.Consumo or Venta.CreditoFiscal))
            return "Revise la cantidad y el tipo de comprobante.";

        var nombre = datos.NombreCliente.Trim();
        var documento = datos.DocumentoCliente.Trim().Replace("-", "");
        if (nombre.Length > 200 || documento.Length > 30)
            return "Revise la longitud de los datos del comprador y del comprobante.";

        await using var db = await factory.CreateDbContextAsync();
        await using var transaccion = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        try
        {
            var venta = datos.Id == 0 ? new Venta { FechaCreacion = AhoraDominicana }
                : await db.Ventas.Include(v => v.Detalles).SingleOrDefaultAsync(v => v.Id == datos.Id && v.Estado == Venta.Borrador);
            if (venta is null) return "Solo se pueden editar borradores.";

            var ids = datos.Detalles.Select(d => d.ArticuloInventarioId).ToList();
            var articulos = await db.ArticulosInventario.AsNoTracking().Where(a => ids.Contains(a.Id)).ToDictionaryAsync(a => a.Id);
            if (articulos.Count != ids.Count) return "Seleccione productos disponibles del inventario.";
            if (datos.ClienteId.HasValue && !await db.Clientes.AnyAsync(c => c.Id == datos.ClienteId))
                return "El cliente seleccionado no está disponible.";

            if (venta.Id > 0) db.DetallesVenta.RemoveRange(venta.Detalles);
            venta.Detalles = datos.Detalles.Select(d =>
            {
                var articulo = articulos[d.ArticuloInventarioId];
                return new DetalleVenta { ArticuloInventarioId = articulo.Id, CodigoArticulo = articulo.Codigo,
                    NombreArticulo = articulo.Nombre, PrecioUnitario = articulo.PrecioUnidad, Cantidad = d.Cantidad };
            }).ToList();
            var primero = venta.Detalles[0];
            venta.ArticuloInventarioId = primero.ArticuloInventarioId;
            venta.ClienteId = datos.ClienteId;
            venta.CodigoArticulo = primero.CodigoArticulo;
            venta.NombreArticulo = primero.NombreArticulo;
            venta.NombreCliente = nombre;
            venta.DocumentoCliente = documento;
            venta.Cantidad = primero.Cantidad;
            venta.PrecioUnitario = primero.PrecioUnitario;
            venta.TasaItbis = 18m;
            venta.TipoComprobante = datos.TipoComprobante;
            var prefijo = venta.TipoComprobante == Venta.CreditoFiscal ? "DEMO-B01-" : "DEMO-B02-";
            if (venta.Ncf is null || !venta.Ncf.StartsWith(prefijo, StringComparison.Ordinal))
            {
                string referencia;
                do { referencia = prefijo + RandomNumberGenerator.GetInt32(0, 10000).ToString("D4"); }
                while (await db.Ventas.AnyAsync(v => v.Ncf == referencia));
                venta.Ncf = referencia;
            }

            if (registrar)
            {
                if (venta.TipoComprobante == Venta.CreditoFiscal &&
                    (nombre.Length == 0 || !Regex.IsMatch(documento, @"\A(?:[0-9]{9}|[0-9]{11})\z")))
                    return "Indique el nombre y el RNC o cédula del comprador para crédito fiscal.";
                if (venta.TipoComprobante == Venta.Consumo && venta.Total >= 250_000m && documento.Length == 0)
                    return "Identifique al comprador en facturas de consumo de RD$ 250,000 o más.";

                foreach (var linea in venta.Detalles)
                {
                    var descontado = await db.ArticulosInventario
                        .Where(a => a.Id == linea.ArticuloInventarioId && a.Cantidad >= linea.Cantidad)
                        .ExecuteUpdateAsync(set => set.SetProperty(a => a.Cantidad, a => a.Cantidad - linea.Cantidad));
                    if (descontado == 0) return $"Stock insuficiente para {linea.NombreArticulo}.";
                }
                venta.Estado = Venta.Registrada;
                venta.FechaVenta = AhoraDominicana;
            }

            if (venta.Id == 0) db.Ventas.Add(venta);
            await db.SaveChangesAsync();
            await transaccion.CommitAsync();
            return null;
        }
        catch (Exception ex) when (ex is DbException or DbUpdateException)
        {
            return "No se pudo guardar la venta. Actualice los datos e intente de nuevo.";
        }
    }

    public async Task<bool> EliminarBorradorAsync(int id)
    {
        if (!appState.EsGerente) return false;
        await using var db = await factory.CreateDbContextAsync();
        return await db.Ventas.Where(v => v.Id == id && v.Estado == Venta.Borrador).ExecuteDeleteAsync() == 1;
    }

    public async Task<bool> AnularAsync(int id)
    {
        if (!appState.EsGerente) return false;
        await using var db = await factory.CreateDbContextAsync();
        await using var transaccion = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        try
        {
            var venta = await db.Ventas.Include(v => v.Detalles).SingleOrDefaultAsync(v => v.Id == id && v.Estado == Venta.Registrada);
            if (venta is null) return false;
            foreach (var linea in venta.Detalles)
            {
                var devuelto = await db.ArticulosInventario
                    .Where(a => a.Id == linea.ArticuloInventarioId && a.Cantidad <= int.MaxValue - linea.Cantidad)
                    .ExecuteUpdateAsync(set => set.SetProperty(a => a.Cantidad, a => a.Cantidad + linea.Cantidad));
                if (devuelto == 0) return false;
            }
            venta.Estado = Venta.Anulada;
            await db.SaveChangesAsync();
            await transaccion.CommitAsync();
            return true;
        }
        catch (Exception ex) when (ex is DbException or DbUpdateException)
        {
            return false;
        }
    }

    public async Task<List<string>> CategoriasInventarioAsync()
    {
        if (!appState.EsGerente) return [];
        await using var db = await factory.CreateDbContextAsync();
        return await db.ArticulosInventario.AsNoTracking().Where(a => a.Categoria != "")
            .Select(a => a.Categoria).Distinct().OrderBy(c => c).ToListAsync();
    }

    public async Task<List<MesVentas>> UltimosTresMesesAsync(string? categoria = null)
    {
        if (!appState.EsGerente) return [];
        var hoy = AhoraDominicana;
        var actual = new DateTime(hoy.Year, hoy.Month, 1);
        var inicio = actual.AddMonths(-2);
        var fin = actual.AddMonths(1);
        await using var db = await factory.CreateDbContextAsync();
        var ventas = await db.Ventas.AsNoTracking()
            .Include(v => v.Detalles)
            .Where(v => v.Estado == Venta.Registrada && v.FechaVenta >= inicio && v.FechaVenta < fin)
            .ToListAsync();
        var articulosCategoria = string.IsNullOrEmpty(categoria) ? null :
            (await db.ArticulosInventario.AsNoTracking().Where(a => a.Categoria == categoria).Select(a => a.Id).ToListAsync()).ToHashSet();
        return Enumerable.Range(0, 3).Select(i =>
        {
            var mes = inicio.AddMonths(i);
            var operaciones = ventas.Where(v => v.FechaVenta!.Value.Year == mes.Year && v.FechaVenta.Value.Month == mes.Month)
                .Select(v => new { Venta = v, Lineas = v.Detalles.Where(d => articulosCategoria is null || articulosCategoria.Contains(d.ArticuloInventarioId)).ToList() })
                .Where(x => x.Lineas.Count > 0).ToList();
            var subtotal = operaciones.Sum(x => x.Lineas.Sum(d => d.Subtotal));
            return new MesVentas(mes, operaciones.Count, operaciones.Sum(x => x.Lineas.Sum(d => (long)d.Cantidad)),
                subtotal, operaciones.Sum(x => Math.Round(x.Lineas.Sum(d => d.Subtotal) * 0.18m, 2, MidpointRounding.AwayFromZero)));
        }).ToList();
    }
}
