using Microsoft.EntityFrameworkCore;
using ElectroGestion.Context;
using ElectroGestion.Models;
using System.Linq.Expressions;

namespace ElectroGestion.Services;

public class SolicitudService(IDbContextFactory<Contexto> contextFactory) : Aplicada1.Core.IService<Solicitud, int>
{
    public async Task<bool> Guardar(Solicitud solicitud)
    {
        if (!await Existe(solicitud.Id))
        {
            return await Insertar(solicitud);
        }
        else
        {
            return await Modificar(solicitud);
        }
    }

    private async Task<bool> Existe(int id)
    {
        await using var contexto = await contextFactory.CreateDbContextAsync();
        return await contexto.Solicitudes.AnyAsync(s => s.Id == id);
    }

    private async Task<bool> Insertar(Solicitud solicitud)
    {
        try
        {
            await using var contexto = await contextFactory.CreateDbContextAsync();
            await using var transaccion = await contexto.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
            if (!await contexto.Servicios.AnyAsync(s => s.Id == solicitud.ServicioId && s.Estado == "Activo"))
                return false;
            solicitud.Cliente = null;
            solicitud.Servicio = null;
            solicitud.EstadoSolicitud = null;
            contexto.Solicitudes.Add(solicitud);
            await contexto.SaveChangesAsync();
            RegistroEventosSolicitud.Agregar(contexto, solicitud.Id, "Solicitud creada", "Se registró la solicitud del servicio.");
            await contexto.SaveChangesAsync();
            await transaccion.CommitAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    private async Task<bool> Modificar(Solicitud solicitud)
    {
        try
        {
            await using var contexto = await contextFactory.CreateDbContextAsync();
            await using var transaccion = await contexto.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
            var existente = await contexto.Solicitudes.FirstOrDefaultAsync(s => s.Id == solicitud.Id);
            if (existente == null) return false;
            if (await contexto.EstadosSolicitud.AnyAsync(e => e.Id == existente.EstadoSolicitudId && e.Nombre == "Finalizada")) return false;
            if (existente.ServicioId != solicitud.ServicioId &&
                !await contexto.Servicios.AnyAsync(s => s.Id == solicitud.ServicioId && s.Estado == "Activo"))
                return false;

            existente.ClienteId = solicitud.ClienteId;
            existente.ServicioId = solicitud.ServicioId;
            existente.Contacto = solicitud.Contacto;
            existente.Direccion = solicitud.Direccion;
            existente.EstadoSolicitudId = solicitud.EstadoSolicitudId;
            RegistroEventosSolicitud.Agregar(contexto, existente.Id, "Solicitud actualizada", "Se actualizaron los datos principales de la solicitud.");

            await contexto.SaveChangesAsync();
            await transaccion.CommitAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<Solicitud?> Buscar(int id)
    {
        await using var contexto = await contextFactory.CreateDbContextAsync();
        return await contexto.Solicitudes
            .Include(s => s.Cliente)
            .Include(s => s.Servicio)
            .Include(s => s.EstadoSolicitud)
            .Include(s => s.Detalles)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<List<Solicitud>> GetList(Expression<Func<Solicitud, bool>> criterio)
    {
        await using var contexto = await contextFactory.CreateDbContextAsync();
        return await contexto.Solicitudes
            .Include(s => s.Cliente)
            .Include(s => s.Servicio)
            .Include(s => s.EstadoSolicitud)
            .Include(s => s.Detalles)
            .Where(criterio)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<bool> Eliminar(int id)
        => await EliminarVarios([id]);

    public async Task<bool> EliminarVarios(IReadOnlyCollection<int> ids)
    {
        if (ids.Count == 0 || ids.Count != ids.Distinct().Count()) return false;
        await using var contexto = await contextFactory.CreateDbContextAsync();
        await using var transaccion = await contexto.Database.BeginTransactionAsync();
        var solicitudes = await contexto.Solicitudes.Include(s => s.EstadoSolicitud).Where(s => ids.Contains(s.Id)).ToListAsync();
        if (solicitudes.Count != ids.Count || solicitudes.Any(s => s.EstadoSolicitudId is 2 or 5 || s.EstadoSolicitud?.Nombre == "Finalizada")) return false;
        var visitas = await contexto.Visitas
            .IgnoreQueryFilters()
            .Where(v => ids.Contains(v.SolicitudId))
            .ToListAsync();
        if (visitas.Any(v => v.Estado is "En proceso" or "En Espera Administración")) return false;

        foreach (var visita in visitas)
        {
            if (!visita.IsDeleted)
            {
                visita.IsDeleted = true;
                visita.EliminadaPorSolicitud = true;
            }
        }

        foreach (var solicitud in solicitudes) solicitud.IsDeleted = true;
        await contexto.SaveChangesAsync();
        await transaccion.CommitAsync();
        return true;
    }

    public async Task<List<Solicitud>> ObtenerEliminadasAsync()
    {
        await using var contexto = await contextFactory.CreateDbContextAsync();
        return await contexto.Solicitudes
            .IgnoreQueryFilters()
            .Where(s => s.IsDeleted)
            .Include(s => s.Cliente)
            .Include(s => s.Servicio)
            .Include(s => s.EstadoSolicitud)
            .Include(s => s.Detalles)
            .Include(s => s.Levantamiento)
            .AsNoTracking()
            .OrderByDescending(s => s.Id)
            .ToListAsync();
    }

    public async Task<bool> RecuperarAsync(int id)
    {
        await using var contexto = await contextFactory.CreateDbContextAsync();
        var solicitud = await contexto.Solicitudes
            .IgnoreQueryFilters()
            .Include(s => s.Cliente)
            .FirstOrDefaultAsync(s => s.Id == id && s.IsDeleted);
        if (solicitud is null || solicitud.Cliente is null || solicitud.Cliente.IsDeleted)
        {
            return false;
        }

        solicitud.IsDeleted = false;
        RegistroEventosSolicitud.Agregar(contexto, solicitud.Id, "Solicitud restaurada", "La solicitud fue restaurada desde eliminadas.");
        var visitasEliminadasEnCascada = await contexto.Visitas
            .IgnoreQueryFilters()
            .Where(v => v.SolicitudId == id && v.EliminadaPorSolicitud)
            .ToListAsync();
        foreach (var visita in visitasEliminadasEnCascada)
        {
            visita.IsDeleted = false;
            visita.EliminadaPorSolicitud = false;
        }

        await contexto.SaveChangesAsync();
        return true;
    }
}
