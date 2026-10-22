using System.Text.Json;
using ElectroGestion.Context;
using ElectroGestion.Models;
using Microsoft.EntityFrameworkCore;

namespace ElectroGestion.Services;

public class LevantamientoService(IDbContextFactory<Contexto> contextFactory)
{
    private const int EstadoPendienteId = 1;
    private const int EstadoEnProcesoId = 2;
    private const int EstadoEnEsperaAdministracionId = 5;

    public async Task<List<Solicitud>> ObtenerSolicitudesProgramadasAsync()
    {
        await using var contexto = await contextFactory.CreateDbContextAsync();
        return await contexto.Solicitudes
            .Where(s => s.EstadoSolicitudId != 3 &&
                s.EstadoSolicitudId != 4 &&
                contexto.Visitas.Any(v => v.SolicitudId == s.Id && v.Estado != "Cancelada"))
            .Include(s => s.Cliente)
            .Include(s => s.Servicio)
            .Include(s => s.EstadoSolicitud)
            .Include(s => s.Levantamiento)
            .AsNoTracking()
            .OrderBy(s => s.Id)
            .ToListAsync();
    }

    public async Task<int> ContarPendientesAsync()
    {
        await using var contexto = await contextFactory.CreateDbContextAsync();
        return await contexto.Visitas
            .Where(v => v.Estado != "Cancelada" &&
                v.Solicitud != null &&
                v.Solicitud.EstadoSolicitudId == EstadoPendienteId)
            .Select(v => v.SolicitudId)
            .Distinct()
            .CountAsync();
    }

    public async Task<Levantamiento?> ObtenerPorSolicitudAsync(int solicitudId)
    {
        await using var contexto = await contextFactory.CreateDbContextAsync();
        return await contexto.Levantamientos
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.SolicitudId == solicitudId);
    }

    public async Task<bool> IniciarAsync(int solicitudId)
    {
        await using var contexto = await contextFactory.CreateDbContextAsync();
        var solicitud = await contexto.Solicitudes
            .Include(s => s.Levantamiento)
            .FirstOrDefaultAsync(s => s.Id == solicitudId);
        if (solicitud is null || !await TieneVisitaActivaAsync(contexto, solicitudId))
        {
            return false;
        }

        if (solicitud.Levantamiento is null)
        {
            solicitud.Levantamiento = new Levantamiento { SolicitudId = solicitudId };
        }

        if (solicitud.EstadoSolicitudId == EstadoPendienteId)
        {
            solicitud.EstadoSolicitudId = EstadoEnProcesoId;
        }

        await ActualizarEstadoVisitaAsync(contexto, solicitudId, "En proceso");
        await contexto.SaveChangesAsync();
        return true;
    }

    public async Task<bool> GuardarBorradorAsync(int solicitudId, Levantamiento datos)
    {
        await using var contexto = await contextFactory.CreateDbContextAsync();
        var solicitud = await contexto.Solicitudes
            .Include(s => s.Levantamiento)
            .FirstOrDefaultAsync(s => s.Id == solicitudId);
        if (solicitud is null || solicitud.EstadoSolicitudId != EstadoEnProcesoId)
        {
            return false;
        }

        var levantamiento = solicitud.Levantamiento ?? new Levantamiento { SolicitudId = solicitudId };
        CopiarDatos(datos, levantamiento);
        levantamiento.BorradorGuardado = true;

        if (solicitud.Levantamiento is null)
        {
            contexto.Levantamientos.Add(levantamiento);
        }

        await contexto.SaveChangesAsync();
        return true;
    }

    public async Task<bool> EnviarAAdministracionAsync(int solicitudId, Levantamiento datos)
    {
        await using var contexto = await contextFactory.CreateDbContextAsync();
        var solicitud = await contexto.Solicitudes
            .Include(s => s.Levantamiento)
            .FirstOrDefaultAsync(s => s.Id == solicitudId);
        if (solicitud?.Levantamiento is null ||
            solicitud.EstadoSolicitudId != EstadoEnProcesoId ||
            !TieneDatosRequeridos(datos))
        {
            return false;
        }

        CopiarDatos(datos, solicitud.Levantamiento);
        solicitud.EstadoSolicitudId = EstadoEnEsperaAdministracionId;
        solicitud.Levantamiento.BorradorGuardado = false;
        await ActualizarEstadoVisitaAsync(contexto, solicitudId, "En Espera Administración");
        await contexto.SaveChangesAsync();
        return true;
    }

    private static bool TieneDatosRequeridos(Levantamiento levantamiento) =>
        !string.IsNullOrWhiteSpace(levantamiento.DisponibilidadEspacios) &&
        !string.IsNullOrWhiteSpace(levantamiento.CargasElectricas) &&
        !string.IsNullOrWhiteSpace(levantamiento.EstabilidadTension) &&
        !string.IsNullOrWhiteSpace(levantamiento.RecorridoTuberias) &&
        !string.IsNullOrWhiteSpace(levantamiento.RecorridoCableado) &&
        JsonSerializer.Deserialize<List<string>>(levantamiento.FotografiasJson)?.Count > 0;

    private static void CopiarDatos(Levantamiento origen, Levantamiento destino)
    {
        destino.DisponibilidadEspacios = origen.DisponibilidadEspacios.Trim();
        destino.CargasElectricas = origen.CargasElectricas.Trim();
        destino.EstabilidadTension = origen.EstabilidadTension.Trim();
        destino.RecorridoTuberias = origen.RecorridoTuberias.Trim();
        destino.RecorridoCableado = origen.RecorridoCableado.Trim();
        destino.Observaciones = origen.Observaciones.Trim();
        destino.FotografiasJson = origen.FotografiasJson;
    }

    private static async Task<bool> TieneVisitaActivaAsync(Contexto contexto, int solicitudId) =>
        await contexto.Visitas.AnyAsync(v => v.SolicitudId == solicitudId && v.Estado != "Cancelada");

    private static async Task ActualizarEstadoVisitaAsync(Contexto contexto, int solicitudId, string estado)
    {
        var visitas = await contexto.Visitas
            .Where(v => v.SolicitudId == solicitudId && v.Estado != "Cancelada")
            .ToListAsync();
        foreach (var visita in visitas)
        {
            visita.Estado = estado;
        }
    }
}
