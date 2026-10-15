using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ElectroGestion.Migrations
{
    /// <inheritdoc />
    public partial class SepararProgramacionSolicitud : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF COL_LENGTH(N'dbo.Solicitudes', N'HoraFin') IS NOT NULL
                BEGIN
                    EXEC(N'
                        INSERT INTO [dbo].[Visitas] ([SolicitudId], [TecnicoId], [Fecha], [Hora], [HoraFin], [Estado])
                        SELECT solicitud.[Id], NULL, solicitud.[Fecha], solicitud.[Hora],
                               COALESCE(
                                   CONVERT(varchar(5), TRY_CONVERT(time(0), solicitud.[HoraFin]), 108),
                                   CONVERT(varchar(5), DATEADD(MINUTE, 150, TRY_CONVERT(time(0), solicitud.[Hora])), 108)),
                               COALESCE(estado.[Nombre], N''Pendiente'')
                        FROM [dbo].[Solicitudes] AS solicitud
                        LEFT JOIN [dbo].[EstadosSolicitud] AS estado
                            ON estado.[Id] = solicitud.[EstadoSolicitudId]
                        WHERE NULLIF(LTRIM(RTRIM(solicitud.[Hora])), N'''') IS NOT NULL
                            AND TRY_CONVERT(time(0), solicitud.[Hora]) IS NOT NULL
                            AND NOT EXISTS (
                                SELECT 1
                                FROM [dbo].[Visitas] AS visita
                                WHERE visita.[SolicitudId] = solicitud.[Id]);
                    ');
                END
                ELSE
                BEGIN
                    INSERT INTO [dbo].[Visitas] ([SolicitudId], [TecnicoId], [Fecha], [Hora], [HoraFin], [Estado])
                    SELECT solicitud.[Id], NULL, solicitud.[Fecha], solicitud.[Hora],
                           CONVERT(varchar(5), DATEADD(MINUTE, 150, TRY_CONVERT(time(0), solicitud.[Hora])), 108),
                           COALESCE(estado.[Nombre], N'Pendiente')
                    FROM [dbo].[Solicitudes] AS solicitud
                    LEFT JOIN [dbo].[EstadosSolicitud] AS estado
                        ON estado.[Id] = solicitud.[EstadoSolicitudId]
                    WHERE NULLIF(LTRIM(RTRIM(solicitud.[Hora])), N'') IS NOT NULL
                        AND TRY_CONVERT(time(0), solicitud.[Hora]) IS NOT NULL
                        AND NOT EXISTS (
                            SELECT 1
                            FROM [dbo].[Visitas] AS visita
                            WHERE visita.[SolicitudId] = solicitud.[Id]);
                END;
                """);

            migrationBuilder.DropColumn(
                name: "Fecha",
                table: "Solicitudes");

            migrationBuilder.DropColumn(
                name: "Hora",
                table: "Solicitudes");

            migrationBuilder.Sql("""
                IF COL_LENGTH(N'dbo.Solicitudes', N'HoraFin') IS NOT NULL
                    ALTER TABLE [dbo].[Solicitudes] DROP COLUMN [HoraFin];
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "Fecha",
                table: "Solicitudes",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Hora",
                table: "Solicitudes",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("""
                IF COL_LENGTH(N'dbo.Solicitudes', N'HoraFin') IS NULL
                    ALTER TABLE [dbo].[Solicitudes] ADD [HoraFin] nvarchar(10) NULL;

                UPDATE solicitud
                SET solicitud.[Fecha] = visita.[Fecha],
                    solicitud.[Hora] = visita.[Hora],
                    solicitud.[HoraFin] = visita.[HoraFin]
                FROM [dbo].[Solicitudes] AS solicitud
                INNER JOIN [dbo].[Visitas] AS visita
                    ON visita.[SolicitudId] = solicitud.[Id];
                """);

            migrationBuilder.UpdateData(
                table: "Solicitudes",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Fecha", "Hora" },
                values: new object[] { new DateTime(2026, 10, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), "09:00" });

            migrationBuilder.UpdateData(
                table: "Solicitudes",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Fecha", "Hora" },
                values: new object[] { new DateTime(2026, 10, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "10:30" });

            migrationBuilder.UpdateData(
                table: "Solicitudes",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Fecha", "Hora" },
                values: new object[] { new DateTime(2026, 10, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), "14:00" });

            migrationBuilder.UpdateData(
                table: "Solicitudes",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Fecha", "Hora" },
                values: new object[] { new DateTime(2026, 10, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "08:30" });

            migrationBuilder.UpdateData(
                table: "Solicitudes",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Fecha", "Hora" },
                values: new object[] { new DateTime(2026, 10, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "11:00" });

            migrationBuilder.UpdateData(
                table: "Solicitudes",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Fecha", "Hora" },
                values: new object[] { new DateTime(2026, 9, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "15:00" });

            migrationBuilder.UpdateData(
                table: "Solicitudes",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "Fecha", "Hora" },
                values: new object[] { new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "09:30" });

            migrationBuilder.UpdateData(
                table: "Solicitudes",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "Fecha", "Hora" },
                values: new object[] { new DateTime(2026, 10, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), "10:00" });
        }
    }
}
