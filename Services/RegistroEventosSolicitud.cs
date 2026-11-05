using ElectroGestion.Context;
using ElectroGestion.Models;

namespace ElectroGestion.Services;

internal static class RegistroEventosSolicitud
{
    public static void Agregar(Contexto db, int solicitudId, string titulo, string detalle) =>
        db.SolicitudesEventos.Add(new SolicitudEvento
        {
            SolicitudId = solicitudId,
            Fecha = DateTime.Now,
            Titulo = titulo,
            Detalle = detalle
        });
}
