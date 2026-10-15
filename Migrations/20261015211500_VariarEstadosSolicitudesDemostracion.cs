using ElectroGestion.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ElectroGestion.Migrations
{
    [DbContext(typeof(Contexto))]
    [Migration("20261015211500_VariarEstadosSolicitudesDemostracion")]
    public partial class VariarEstadosSolicitudesDemostracion : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DECLARE @EstadosDemostracion TABLE
                (
                    [ClienteId] int NOT NULL,
                    [ServicioId] int NOT NULL,
                    [Fecha] date NOT NULL,
                    [Hora] nvarchar(10) NOT NULL,
                    [TecnicoAsignado] nvarchar(150) NOT NULL,
                    [Estado] nvarchar(50) NOT NULL
                );

                INSERT INTO @EstadosDemostracion
                    ([ClienteId], [ServicioId], [Fecha], [Hora], [TecnicoAsignado], [Estado])
                VALUES
                    (1, 2, '20261015', N'10:30', N'Luis Peña', N'Pendiente'),
                    (2, 1, '20261015', N'13:30', N'Carlos Méndez', N'Finalizada'),
                    (3, 3, '20261016', N'10:30', N'Carlos Méndez', N'En proceso'),
                    (4, 4, '20261016', N'13:30', N'Luis Peña', N'Pendiente'),
                    (5, 5, '20261017', N'10:30', N'Luis Peña', N'Pendiente'),
                    (1, 3, '20261017', N'13:30', N'Carlos Méndez', N'En proceso'),
                    (2, 4, '20261019', N'10:30', N'Luis Peña', N'Pendiente'),
                    (3, 2, '20261019', N'13:30', N'Carlos Méndez', N'En proceso'),
                    (4, 1, '20261020', N'10:30', N'Carlos Méndez', N'Pendiente'),
                    (5, 3, '20261020', N'13:30', N'Luis Peña', N'Cancelada'),
                    (1, 5, '20261021', N'10:30', N'Luis Peña', N'Pendiente'),
                    (2, 3, '20261021', N'13:30', N'Carlos Méndez', N'En proceso'),
                    (3, 4, '20261022', N'10:30', N'Carlos Méndez', N'Pendiente'),
                    (4, 2, '20261022', N'13:30', N'Luis Peña', N'Pendiente'),
                    (5, 1, '20261023', N'10:30', N'Luis Peña', N'Cancelada'),
                    (1, 4, '20261023', N'13:30', N'Carlos Méndez', N'En proceso'),
                    (2, 5, '20261024', N'10:30', N'Carlos Méndez', N'Pendiente'),
                    (3, 1, '20261024', N'13:30', N'Luis Peña', N'En proceso');

                UPDATE solicitud
                SET [EstadoSolicitudId] = estadoSolicitud.[Id]
                FROM [Solicitudes] AS solicitud
                INNER JOIN @EstadosDemostracion AS muestra
                    ON muestra.[ClienteId] = solicitud.[ClienteId]
                    AND muestra.[ServicioId] = solicitud.[ServicioId]
                    AND muestra.[Fecha] = solicitud.[Fecha]
                    AND muestra.[Hora] = solicitud.[Hora]
                    AND muestra.[TecnicoAsignado] = solicitud.[TecnicoAsignado]
                INNER JOIN [EstadosSolicitud] AS estadoSolicitud
                    ON estadoSolicitud.[Nombre] = muestra.[Estado]
                WHERE solicitud.[FechaSolicitud] = '20261014';

                IF @@ROWCOUNT <> 18
                    THROW 51003, N'No se pudieron actualizar los estados de las 18 solicitudes de demostración.', 1;

                UPDATE visita
                SET [Estado] = muestra.[Estado]
                FROM [Visitas] AS visita
                INNER JOIN [Solicitudes] AS solicitud ON solicitud.[Id] = visita.[SolicitudId]
                INNER JOIN @EstadosDemostracion AS muestra
                    ON muestra.[ClienteId] = solicitud.[ClienteId]
                    AND muestra.[ServicioId] = solicitud.[ServicioId]
                    AND muestra.[Fecha] = solicitud.[Fecha]
                    AND muestra.[Hora] = solicitud.[Hora]
                    AND muestra.[TecnicoAsignado] = solicitud.[TecnicoAsignado]
                WHERE solicitud.[FechaSolicitud] = '20261014';

                IF @@ROWCOUNT <> 16
                    THROW 51004, N'No se pudieron actualizar los estados de las 16 visitas de demostración.', 1;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE estadoSolicitud
                SET [EstadoSolicitudId] = pendiente.[Id]
                FROM [Solicitudes] AS estadoSolicitud
                INNER JOIN [EstadosSolicitud] AS pendiente ON pendiente.[Nombre] = N'Pendiente'
                WHERE estadoSolicitud.[FechaSolicitud] = '20261014'
                    AND estadoSolicitud.[TecnicoAsignado] IN (N'Luis Peña', N'Carlos Méndez');

                UPDATE visita
                SET [Estado] = N'Programada'
                FROM [Visitas] AS visita
                INNER JOIN [Solicitudes] AS solicitud ON solicitud.[Id] = visita.[SolicitudId]
                WHERE solicitud.[FechaSolicitud] = '20261014'
                    AND solicitud.[TecnicoAsignado] IN (N'Luis Peña', N'Carlos Méndez');
                """);
        }
    }
}
