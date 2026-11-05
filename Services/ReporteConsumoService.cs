using ElectroGestion.Context;
using Microsoft.EntityFrameworkCore;

namespace ElectroGestion.Services;

public sealed record FilaConsumoServicio(string Servicio, decimal Materiales, decimal Consumibles, int Instalaciones, decimal RiesgoInventario)
{
    public decimal Total => Materiales + Consumibles;
}

public sealed record MesConsumoServicio(string Servicio, int NumeroMes, string Etiqueta, decimal Total);

public sealed record DatosReporteConsumo(IReadOnlyList<FilaConsumoServicio> Servicios, IReadOnlyList<MesConsumoServicio> Meses);

public class ReporteConsumoService(IDbContextFactory<Contexto> contextFactory)
{
    public async Task<DatosReporteConsumo> ObtenerAsync(int anio)
    {
        await using var db = await contextFactory.CreateDbContextAsync();
        var ordenes = await db.OrdenesTrabajo.AsNoTracking()
            .Where(o => o.Estado == "Finalizada")
            .Include(o => o.Cotizacion).ThenInclude(c => c!.Detalles)
            .Include(o => o.Cotizacion).ThenInclude(c => c!.Solicitud).ThenInclude(s => s!.Servicio)
            .ToListAsync();

        var terminadas = ordenes
            .Select(o => new
            {
                Orden = o,
                Servicio = o.Cotizacion?.Solicitud?.Servicio?.Nombre ?? o.TipoServicio,
                Fecha = o.Cotizacion?.Solicitud?.FechaFin ?? o.FechaCreacion,
                Materiales = o.Cotizacion?.Detalles.Sum(d => d.Cantidad * d.PrecioUnitario) ?? 0m
            })
            .Where(x => x.Fecha.Year == anio)
            .ToList();

        var abiertas = await db.OrdenesTrabajo.AsNoTracking()
            .Where(o => o.Estado == "En proceso")
            .Include(o => o.Cotizacion).ThenInclude(c => c!.Detalles)
            .Include(o => o.Cotizacion).ThenInclude(c => c!.Solicitud).ThenInclude(s => s!.Servicio)
            .ToListAsync();
        var riesgoPorServicio = abiertas.GroupBy(o => o.Cotizacion?.Solicitud?.Servicio?.Nombre ?? o.TipoServicio)
            .ToDictionary(g => g.Key, g => g.Sum(o => o.Cotizacion?.Detalles.Sum(d => d.Cantidad * d.PrecioUnitario) ?? 0m));

        var totales = terminadas.GroupBy(x => x.Servicio)
            .ToDictionary(g => g.Key, g => (Materiales: g.Sum(x => x.Materiales), Instalaciones: g.Count()));
        var nombres = await db.Servicios.AsNoTracking().Select(s => s.Nombre).ToListAsync();
        foreach (var nombre in totales.Keys)
            if (!nombres.Contains(nombre, StringComparer.OrdinalIgnoreCase)) nombres.Add(nombre);

        var filas = nombres.OrderBy(n => n).Select(nombre =>
        {
            totales.TryGetValue(nombre, out var valor);
            riesgoPorServicio.TryGetValue(nombre, out var riesgo);
            return new FilaConsumoServicio(nombre, valor.Materiales, 0m, valor.Instalaciones, riesgo);
        }).ToList();

        var meses = filas.SelectMany(fila => Enumerable.Range(1, 12).Select(mes =>
        {
            var fecha = new DateTime(anio, mes, 1);
            return new MesConsumoServicio(fila.Servicio, mes,
                fecha.ToString("MMM", System.Globalization.CultureInfo.GetCultureInfo("es-DO")),
                terminadas.Where(x => x.Servicio == fila.Servicio && x.Fecha.Month == mes).Sum(x => x.Materiales));
        })).ToList();

        return new DatosReporteConsumo(filas, meses);
    }
}
