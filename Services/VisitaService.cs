using ElectroGestion.Context;
using ElectroGestion.Models;
using Microsoft.EntityFrameworkCore;

namespace ElectroGestion.Services;

public class VisitaService(IDbContextFactory<Contexto> contextFactory)
{
    public async Task<List<Visita>> ObtenerTodasAsync()
    {
        await using var contexto = await contextFactory.CreateDbContextAsync();
        return await contexto.Visitas
            .Include(v => v.Solicitud!)
                .ThenInclude(s => s.Cliente)
            .Include(v => v.Solicitud!)
                .ThenInclude(s => s.Servicio)
            .Include(v => v.Solicitud!)
                .ThenInclude(s => s.EstadoSolicitud)
            .Include(v => v.Tecnico)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<Usuario>> ObtenerTecnicosAsync()
    {
        await using var contexto = await contextFactory.CreateDbContextAsync();
        return await contexto.Usuarios
            .Where(u => u.Rol == "Técnico")
            .OrderBy(u => u.Nombre)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<bool> SolicitudDisponibleAsync(int solicitudId, int? visitaId = null)
    {
        await using var contexto = await contextFactory.CreateDbContextAsync();
        return !await contexto.Visitas.AnyAsync(v =>
            v.SolicitudId == solicitudId && (!visitaId.HasValue || v.Id != visitaId.Value));
    }

    public async Task<bool> HorarioDisponibleAsync(
        DateTime fecha,
        string horaInicio,
        string horaFin,
        int? visitaId = null)
    {
        if (!TryGetIntervaloValido(horaInicio, horaFin, out var inicio, out var fin) ||
            fecha.DayOfWeek == DayOfWeek.Sunday)
        {
            return false;
        }

        await using var contexto = await contextFactory.CreateDbContextAsync();
        var horariosExistentes = await contexto.Visitas
            .Where(v => v.Fecha.Date == fecha.Date &&
                (!visitaId.HasValue || v.Id != visitaId.Value))
            .ToListAsync();

        return !horariosExistentes.Any(v =>
            TryGetIntervaloValido(v.Hora, v.HoraFin, out var inicioExistente, out var finExistente) &&
            inicio < finExistente && fin > inicioExistente);
    }

    public async Task<bool> GuardarAsync(Visita visita)
    {
        await using var contexto = await contextFactory.CreateDbContextAsync();
        if (visita.Fecha.DayOfWeek == DayOfWeek.Sunday ||
            !TryGetIntervaloValido(visita.Hora, visita.HoraFin, out var inicio, out var fin))
        {
            return false;
        }

        if (visita.TecnicoId.HasValue &&
            !await contexto.Usuarios.AnyAsync(u => u.Id == visita.TecnicoId && u.Rol == "Técnico"))
        {
            return false;
        }

        var horariosExistentes = await contexto.Visitas
            .Where(v => v.Fecha.Date == visita.Fecha.Date && v.Id != visita.Id)
            .ToListAsync();
        if (horariosExistentes.Any(v =>
                TryGetIntervaloValido(v.Hora, v.HoraFin, out var inicioExistente, out var finExistente) &&
                inicio < finExistente && fin > inicioExistente))
        {
            return false;
        }

        if (visita.Id == 0)
        {
            if (await contexto.Visitas.AnyAsync(v => v.SolicitudId == visita.SolicitudId))
            {
                return false;
            }

            visita.Solicitud = null;
            contexto.Visitas.Add(visita);
        }
        else
        {
            var existente = await contexto.Visitas.FirstOrDefaultAsync(v => v.Id == visita.Id);
            if (existente is null)
            {
                return false;
            }

            if (existente.SolicitudId != visita.SolicitudId &&
                await contexto.Visitas.AnyAsync(v =>
                    v.SolicitudId == visita.SolicitudId && v.Id != visita.Id))
            {
                return false;
            }

            existente.SolicitudId = visita.SolicitudId;
            existente.TecnicoId = visita.TecnicoId;
            existente.Fecha = visita.Fecha;
            existente.Hora = visita.Hora;
            existente.HoraFin = visita.HoraFin;
            existente.Estado = visita.Estado;
        }

        return await contexto.SaveChangesAsync() > 0;
    }

    private static bool TryGetIntervaloValido(
        string horaInicio,
        string horaFin,
        out TimeOnly inicio,
        out TimeOnly fin)
    {
        inicio = default;
        fin = default;
        return TimeOnly.TryParse(horaInicio, out inicio) &&
            TimeOnly.TryParse(horaFin, out fin) &&
            inicio >= new TimeOnly(8, 0) &&
            fin <= new TimeOnly(18, 0) &&
            fin > inicio;
    }

    public async Task<bool> EliminarAsync(int id)
    {
        await using var contexto = await contextFactory.CreateDbContextAsync();
        return await contexto.Visitas
            .Where(v => v.Id == id)
            .ExecuteDeleteAsync() > 0;
    }

    public async Task<bool> CambiarEstadoAsync(int id, string estado)
    {
        await using var contexto = await contextFactory.CreateDbContextAsync();
        var visita = await contexto.Visitas.FirstOrDefaultAsync(v => v.Id == id);
        if (visita is null)
        {
            return false;
        }

        visita.Estado = estado;
        return await contexto.SaveChangesAsync() > 0;
    }
}
