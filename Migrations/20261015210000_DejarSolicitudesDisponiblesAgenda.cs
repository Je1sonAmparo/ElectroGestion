using ElectroGestion.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ElectroGestion.Migrations
{
    [DbContext(typeof(Contexto))]
    [Migration("20261015210000_DejarSolicitudesDisponiblesAgenda")]
    public partial class DejarSolicitudesDisponiblesAgenda : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE visita
                FROM [Visitas] AS visita
                INNER JOIN [Solicitudes] AS solicitud ON solicitud.[Id] = visita.[SolicitudId]
                WHERE solicitud.[FechaSolicitud] = '20261014'
                    AND (
                        (
                            solicitud.[ClienteId] = 2
                            AND solicitud.[ServicioId] = 4
                            AND solicitud.[Fecha] = '20261019'
                            AND solicitud.[Hora] = N'10:30'
                            AND solicitud.[TecnicoAsignado] = N'Luis Peña'
                        )
                        OR
                        (
                            solicitud.[ClienteId] = 3
                            AND solicitud.[ServicioId] = 4
                            AND solicitud.[Fecha] = '20261022'
                            AND solicitud.[Hora] = N'10:30'
                            AND solicitud.[TecnicoAsignado] = N'Carlos Méndez'
                        )
                    );

                IF @@ROWCOUNT <> 2
                    THROW 51002, N'No se pudieron dejar exactamente tres solicitudes disponibles en la agenda.', 1;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                INSERT INTO [Visitas] ([SolicitudId], [TecnicoId], [Fecha], [Hora], [HoraFin], [Estado])
                SELECT solicitud.[Id], tecnico.[Id], solicitud.[Fecha], solicitud.[Hora], N'12:00', N'Programada'
                FROM [Solicitudes] AS solicitud
                INNER JOIN [Usuarios] AS tecnico
                    ON tecnico.[UsuarioLogin] = N'tecnico'
                    AND tecnico.[Nombre] = N'Luis Peña'
                    AND tecnico.[Rol] = N'Técnico'
                WHERE solicitud.[FechaSolicitud] = '20261014'
                    AND solicitud.[ClienteId] = 2
                    AND solicitud.[ServicioId] = 4
                    AND solicitud.[Fecha] = '20261019'
                    AND solicitud.[Hora] = N'10:30'
                    AND solicitud.[TecnicoAsignado] = N'Luis Peña'
                    AND NOT EXISTS (
                        SELECT 1 FROM [Visitas] AS visita
                        WHERE visita.[SolicitudId] = solicitud.[Id]);

                INSERT INTO [Visitas] ([SolicitudId], [TecnicoId], [Fecha], [Hora], [HoraFin], [Estado])
                SELECT solicitud.[Id], tecnico.[Id], solicitud.[Fecha], solicitud.[Hora], N'12:00', N'Programada'
                FROM [Solicitudes] AS solicitud
                INNER JOIN [Usuarios] AS tecnico
                    ON tecnico.[UsuarioLogin] = N'carlos'
                    AND tecnico.[Nombre] = N'Carlos Méndez'
                    AND tecnico.[Rol] = N'Técnico'
                WHERE solicitud.[FechaSolicitud] = '20261014'
                    AND solicitud.[ClienteId] = 3
                    AND solicitud.[ServicioId] = 4
                    AND solicitud.[Fecha] = '20261022'
                    AND solicitud.[Hora] = N'10:30'
                    AND solicitud.[TecnicoAsignado] = N'Carlos Méndez'
                    AND NOT EXISTS (
                        SELECT 1 FROM [Visitas] AS visita
                        WHERE visita.[SolicitudId] = solicitud.[Id]);
                """);
        }
    }
}
