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
            .Where(v => !v.IsDeleted)
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

    public async Task<List<Visita>> ObtenerEliminadasAsync()
    {
        await using var contexto = await contextFactory.CreateDbContextAsync();
        return await contexto.Visitas
            .IgnoreQueryFilters()
            .Where(v => v.IsDeleted && v.Solicitud != null && !v.Solicitud.IsDeleted)
            .Include(v => v.Solicitud!)
                .ThenInclude(s => s.Cliente)
            .Include(v => v.Solicitud!)
                .ThenInclude(s => s.Servicio)
            .Include(v => v.Solicitud!)
                .ThenInclude(s => s.EstadoSolicitud)
            .Include(v => v.Tecnico)
            .AsNoTracking()
            .OrderByDescending(v => v.Id)
            .ToListAsync();
    }

    public async Task<bool> RecuperarAsync(int id)
    {
        await using var contexto = await contextFactory.CreateDbContextAsync();
        var visita = await contexto.Visitas
            .IgnoreQueryFilters()
            .Include(v => v.Solicitud)
            .FirstOrDefaultAsync(v => v.Id == id && v.IsDeleted);
        if (visita?.Solicitud is null || visita.Solicitud.IsDeleted || visita.EliminadaPorSolicitud)
        {
            return false;
        }

        visita.IsDeleted = false;
        return await contexto.SaveChangesAsync() > 0;
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
            !v.IsDeleted &&
            v.SolicitudId == solicitudId && (!visitaId.HasValue || v.Id != visitaId.Value));
    }

    public async Task<bool> ActualizarTecnicoAsignadoAsync(int visitaId, int? tecnicoId)
    {
        await using var contexto = await contextFactory.CreateDbContextAsync();
        var visita = await contexto.Visitas.FirstOrDefaultAsync(v => v.Id == visitaId);
        if (visita is null)
        {
            return false;
        }

        if (tecnicoId.HasValue &&
            !await contexto.Usuarios.AnyAsync(u => u.Id == tecnicoId && u.Rol == "Técnico"))
        {
            return false;
        }

        if (visita.Estado != "Cancelada")
        {
            var visitasMismoTecnico = await contexto.Visitas
                .Where(v => v.Id != visitaId &&
                    !v.IsDeleted &&
                    v.Estado != "Cancelada" &&
                    v.Fecha.Date == visita.Fecha.Date &&
                    v.TecnicoId == tecnicoId)
                .ToListAsync();

            if (!TryGetIntervaloValido(visita.Hora, visita.HoraFin, out var inicio, out var fin) ||
                visitasMismoTecnico.Any(v =>
                    TryGetIntervaloValido(v.Hora, v.HoraFin, out var inicioExistente, out var finExistente) &&
                    inicio < finExistente && fin > inicioExistente))
            {
                return false;
            }
        }

        visita.TecnicoId = tecnicoId;
        return await contexto.SaveChangesAsync() > 0;
    }

    public async Task<bool> HorarioDisponibleAsync(
        DateTime fecha,
        string horaInicio,
        string horaFin,
        int? visitaId = null,
        int? tecnicoId = null)
    {
        if (!TryGetIntervaloValido(horaInicio, horaFin, out var inicio, out var fin) ||
            fecha.DayOfWeek == DayOfWeek.Sunday)
        {
            return false;
        }

        await using var contexto = await contextFactory.CreateDbContextAsync();
        var query = contexto.Visitas
            .Where(v => !v.IsDeleted &&
                        v.Estado != "Cancelada" &&
                        v.Fecha.Date == fecha.Date &&
                        (!visitaId.HasValue || v.Id != visitaId.Value));

        if (tecnicoId.HasValue)
        {
            query = query.Where(v => v.TecnicoId == tecnicoId.Value);
        }
        else
        {
            query = query.Where(v => v.TecnicoId == null);
        }

        var horariosExistentes = await query.ToListAsync();

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

        var query = contexto.Visitas
            .Where(v => !v.IsDeleted &&
                        v.Estado != "Cancelada" &&
                        v.Fecha.Date == visita.Fecha.Date &&
                        v.Id != visita.Id);

        if (visita.TecnicoId.HasValue)
        {
            query = query.Where(v => v.TecnicoId == visita.TecnicoId.Value);
        }
        else
        {
            query = query.Where(v => v.TecnicoId == null);
        }

        var horariosExistentes = await query.ToListAsync();
        if (horariosExistentes.Any(v =>
                TryGetIntervaloValido(v.Hora, v.HoraFin, out var inicioExistente, out var finExistente) &&
                inicio < finExistente && fin > inicioExistente))
        {
            return false;
        }

        if (visita.Id == 0)
        {
            if (await contexto.Visitas.AnyAsync(v => !v.IsDeleted && v.SolicitudId == visita.SolicitudId))
            {
                return false;
            }

            var solicitud = await contexto.Solicitudes.FirstOrDefaultAsync(s => s.Id == visita.SolicitudId);
            if (solicitud != null)
            {
                if (solicitud.EstadoSolicitudId == 1) // Pendiente -> En proceso
                {
                    solicitud.EstadoSolicitudId = 2;
                }
            }

            if (string.IsNullOrEmpty(visita.Estado) || visita.Estado == "Pendiente")
            {
                visita.Estado = "En proceso";
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
                    !v.IsDeleted && v.SolicitudId == visita.SolicitudId && v.Id != visita.Id))
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

        if (!TimeOnly.TryParse(horaInicio, out inicio))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(horaFin) || !TimeOnly.TryParse(horaFin, out fin))
        {
            fin = inicio.AddHours(1);
        }

        return inicio >= new TimeOnly(8, 0) &&
            fin <= new TimeOnly(18, 0) &&
            fin > inicio;
    }

    public async Task<bool> EliminarAsync(int id)
    {
        await using var contexto = await contextFactory.CreateDbContextAsync();
        var visita = await contexto.Visitas
            .Include(v => v.Solicitud)
            .FirstOrDefaultAsync(v => v.Id == id);
        if (visita is null ||
            visita.Estado is "En proceso" or "En Espera Administración" ||
            visita.Solicitud?.EstadoSolicitudId is 2 or 5)
        {
            return false;
        }

        visita.IsDeleted = true;
        visita.EliminadaPorSolicitud = false;

        return await contexto.SaveChangesAsync() > 0;
    }

    public async Task<bool> CambiarEstadoAsync(int id, string estado)
    {
        await using var contexto = await contextFactory.CreateDbContextAsync();
        var visita = await contexto.Visitas
            .Include(v => v.Solicitud)
            .FirstOrDefaultAsync(v => v.Id == id);
        if (visita is null)
        {
            return false;
        }

        if (visita.Solicitud is not null)
        {
            if (estado == "Cancelada")
            {
                visita.Solicitud.EstadoSolicitudId = 4;
            }
            else if (estado == "En proceso" && visita.Estado == "Cancelada")
            {
                visita.Solicitud.EstadoSolicitudId = 2;
            }
        }

        visita.Estado = estado;
        var solicitud = await contexto.Solicitudes.FirstOrDefaultAsync(s => s.Id == visita.SolicitudId);
        if (solicitud != null)
        {
            if (estado == "En proceso" && solicitud.EstadoSolicitudId != 2)
            {
                solicitud.EstadoSolicitudId = 2;
            }
            else if (estado == "Finalizada" && solicitud.EstadoSolicitudId != 3)
            {
                solicitud.EstadoSolicitudId = 3;
            }
            else if (estado == "Cancelada" && solicitud.EstadoSolicitudId == 2)
            {
                var tieneOtraActiva = await contexto.Visitas.AnyAsync(v => v.SolicitudId == visita.SolicitudId && v.Id != id && !v.IsDeleted && v.Estado != "Cancelada");
                if (!tieneOtraActiva)
                {
                    solicitud.EstadoSolicitudId = 1;
                }
            }
        }

        return await contexto.SaveChangesAsync() > 0;
    }
}
