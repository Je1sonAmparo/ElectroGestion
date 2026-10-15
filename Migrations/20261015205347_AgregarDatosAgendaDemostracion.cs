using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ElectroGestion.Migrations
{
    /// <inheritdoc />
    public partial class AgregarDatosAgendaDemostracion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DECLARE @TecnicoLuisId int;
                SELECT @TecnicoLuisId = [Id]
                FROM [Usuarios]
                WHERE [UsuarioLogin] = N'tecnico' AND [Rol] = N'Técnico';

                IF @TecnicoLuisId IS NULL
                    THROW 51000, N'No se encontró al técnico Luis Peña para crear los datos de agenda.', 1;

                DECLARE @TecnicoCarlosId int;
                SELECT @TecnicoCarlosId = [Id]
                FROM [Usuarios]
                WHERE [UsuarioLogin] = N'carlos';

                IF @TecnicoCarlosId IS NULL
                BEGIN
                    INSERT INTO [Usuarios] ([UsuarioLogin], [PasswordHash], [Nombre], [Cargo], [Rol])
                    VALUES (
                        N'carlos',
                        N'AQAAAAIAAYagAAAAEMB/vjFzmu7a6+6obTRsJEzOzWZkBqfG9Sy1qVmJ/d1f0CP61CbwqUdTOQM0lzd+pQ==',
                        N'Carlos Méndez',
                        N'Técnico',
                        N'Técnico');
                    SET @TecnicoCarlosId = CONVERT(int, SCOPE_IDENTITY());
                END
                ELSE IF NOT EXISTS (
                    SELECT 1
                    FROM [Usuarios]
                    WHERE [Id] = @TecnicoCarlosId
                        AND [Nombre] = N'Carlos Méndez'
                        AND [Rol] = N'Técnico')
                BEGIN
                    THROW 51001, N'El usuario carlos ya existe con datos incompatibles con el técnico de demostración.', 1;
                END;

                DECLARE @NuevasSolicitudes TABLE
                (
                    [Numero] int PRIMARY KEY,
                    [ClienteId] int NOT NULL,
                    [ServicioId] int NOT NULL,
                    [Contacto] nvarchar(50) NOT NULL,
                    [Direccion] nvarchar(250) NOT NULL,
                    [Fecha] date NOT NULL,
                    [Hora] nvarchar(10) NOT NULL,
                    [HoraFin] nvarchar(10) NOT NULL,
                    [TecnicoId] int NOT NULL,
                    [TecnicoAsignado] nvarchar(150) NOT NULL
                );

                INSERT INTO @NuevasSolicitudes
                    ([Numero], [ClienteId], [ServicioId], [Contacto], [Direccion], [Fecha], [Hora], [HoraFin], [TecnicoId], [TecnicoAsignado])
                VALUES
                    (1, 1, 2, N'Melody Santos', N'Calle Duarte #24, Santiago', '20261015', N'10:30', N'12:00', @TecnicoLuisId, N'Luis Peña'),
                    (2, 2, 1, N'Constructora López', N'Av. Estrella Sadhalá #102, Santiago', '20261015', N'13:30', N'15:00', @TecnicoCarlosId, N'Carlos Méndez'),
                    (3, 3, 3, N'Farmacia El Sol', N'Calle 16 #7, La Vega', '20261016', N'10:30', N'12:00', @TecnicoCarlosId, N'Carlos Méndez'),
                    (4, 4, 4, N'Colmado Hernández', N'Calle Principal #58, Moca', '20261016', N'13:30', N'15:00', @TecnicoLuisId, N'Luis Peña'),
                    (5, 5, 5, N'Taller Mecánico RD', N'Carretera Luperón km 5, Santiago', '20261017', N'10:30', N'12:00', @TecnicoLuisId, N'Luis Peña'),
                    (6, 1, 3, N'Melody Santos', N'Calle Duarte #24, Santiago', '20261017', N'13:30', N'15:00', @TecnicoCarlosId, N'Carlos Méndez'),
                    (7, 2, 4, N'Constructora López', N'Av. Estrella Sadhalá #102, Santiago', '20261019', N'10:30', N'12:00', @TecnicoLuisId, N'Luis Peña'),
                    (8, 3, 2, N'Farmacia El Sol', N'Calle 16 #7, La Vega', '20261019', N'13:30', N'15:00', @TecnicoCarlosId, N'Carlos Méndez'),
                    (9, 4, 1, N'Colmado Hernández', N'Calle Principal #58, Moca', '20261020', N'10:30', N'12:00', @TecnicoCarlosId, N'Carlos Méndez'),
                    (10, 5, 3, N'Taller Mecánico RD', N'Carretera Luperón km 5, Santiago', '20261020', N'13:30', N'15:00', @TecnicoLuisId, N'Luis Peña'),
                    (11, 1, 5, N'Melody Santos', N'Calle Duarte #24, Santiago', '20261021', N'10:30', N'12:00', @TecnicoLuisId, N'Luis Peña'),
                    (12, 2, 3, N'Constructora López', N'Av. Estrella Sadhalá #102, Santiago', '20261021', N'13:30', N'15:00', @TecnicoCarlosId, N'Carlos Méndez'),
                    (13, 3, 4, N'Farmacia El Sol', N'Calle 16 #7, La Vega', '20261022', N'10:30', N'12:00', @TecnicoCarlosId, N'Carlos Méndez'),
                    (14, 4, 2, N'Colmado Hernández', N'Calle Principal #58, Moca', '20261022', N'13:30', N'15:00', @TecnicoLuisId, N'Luis Peña'),
                    (15, 5, 1, N'Taller Mecánico RD', N'Carretera Luperón km 5, Santiago', '20261023', N'10:30', N'12:00', @TecnicoLuisId, N'Luis Peña'),
                    (16, 1, 4, N'Melody Santos', N'Calle Duarte #24, Santiago', '20261023', N'13:30', N'15:00', @TecnicoCarlosId, N'Carlos Méndez'),
                    (17, 2, 5, N'Constructora López', N'Av. Estrella Sadhalá #102, Santiago', '20261024', N'10:30', N'12:00', @TecnicoCarlosId, N'Carlos Méndez'),
                    (18, 3, 1, N'Farmacia El Sol', N'Calle 16 #7, La Vega', '20261024', N'13:30', N'15:00', @TecnicoLuisId, N'Luis Peña');

                DECLARE @MapaSolicitudes TABLE ([Numero] int PRIMARY KEY, [SolicitudId] int NOT NULL);

                MERGE [Solicitudes] AS destino
                USING @NuevasSolicitudes AS origen ON 1 = 0
                WHEN NOT MATCHED THEN
                    INSERT ([ClienteId], [ServicioId], [Contacto], [Direccion], [FechaSolicitud], [Fecha], [Hora], [EstadoSolicitudId], [TecnicoAsignado])
                    VALUES (origen.[ClienteId], origen.[ServicioId], origen.[Contacto], origen.[Direccion], '20261014', origen.[Fecha], origen.[Hora], 1, origen.[TecnicoAsignado])
                OUTPUT origen.[Numero], inserted.[Id] INTO @MapaSolicitudes ([Numero], [SolicitudId]);

                INSERT INTO [Visitas] ([SolicitudId], [TecnicoId], [Fecha], [Hora], [HoraFin], [Estado])
                SELECT mapa.[SolicitudId], datos.[TecnicoId], datos.[Fecha], datos.[Hora], datos.[HoraFin], N'Programada'
                FROM @NuevasSolicitudes AS datos
                INNER JOIN @MapaSolicitudes AS mapa ON mapa.[Numero] = datos.[Numero];
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DECLARE @DatosSemilla TABLE
                (
                    [ClienteId] int NOT NULL,
                    [ServicioId] int NOT NULL,
                    [Contacto] nvarchar(50) NOT NULL,
                    [Direccion] nvarchar(250) NOT NULL,
                    [Fecha] date NOT NULL,
                    [Hora] nvarchar(10) NOT NULL,
                    [TecnicoAsignado] nvarchar(150) NOT NULL
                );

                INSERT INTO @DatosSemilla
                    ([ClienteId], [ServicioId], [Contacto], [Direccion], [Fecha], [Hora], [TecnicoAsignado])
                VALUES
                    (1, 2, N'Melody Santos', N'Calle Duarte #24, Santiago', '20261015', N'10:30', N'Luis Peña'),
                    (2, 1, N'Constructora López', N'Av. Estrella Sadhalá #102, Santiago', '20261015', N'13:30', N'Carlos Méndez'),
                    (3, 3, N'Farmacia El Sol', N'Calle 16 #7, La Vega', '20261016', N'10:30', N'Carlos Méndez'),
                    (4, 4, N'Colmado Hernández', N'Calle Principal #58, Moca', '20261016', N'13:30', N'Luis Peña'),
                    (5, 5, N'Taller Mecánico RD', N'Carretera Luperón km 5, Santiago', '20261017', N'10:30', N'Luis Peña'),
                    (1, 3, N'Melody Santos', N'Calle Duarte #24, Santiago', '20261017', N'13:30', N'Carlos Méndez'),
                    (2, 4, N'Constructora López', N'Av. Estrella Sadhalá #102, Santiago', '20261019', N'10:30', N'Luis Peña'),
                    (3, 2, N'Farmacia El Sol', N'Calle 16 #7, La Vega', '20261019', N'13:30', N'Carlos Méndez'),
                    (4, 1, N'Colmado Hernández', N'Calle Principal #58, Moca', '20261020', N'10:30', N'Carlos Méndez'),
                    (5, 3, N'Taller Mecánico RD', N'Carretera Luperón km 5, Santiago', '20261020', N'13:30', N'Luis Peña'),
                    (1, 5, N'Melody Santos', N'Calle Duarte #24, Santiago', '20261021', N'10:30', N'Luis Peña'),
                    (2, 3, N'Constructora López', N'Av. Estrella Sadhalá #102, Santiago', '20261021', N'13:30', N'Carlos Méndez'),
                    (3, 4, N'Farmacia El Sol', N'Calle 16 #7, La Vega', '20261022', N'10:30', N'Carlos Méndez'),
                    (4, 2, N'Colmado Hernández', N'Calle Principal #58, Moca', '20261022', N'13:30', N'Luis Peña'),
                    (5, 1, N'Taller Mecánico RD', N'Carretera Luperón km 5, Santiago', '20261023', N'10:30', N'Luis Peña'),
                    (1, 4, N'Melody Santos', N'Calle Duarte #24, Santiago', '20261023', N'13:30', N'Carlos Méndez'),
                    (2, 5, N'Constructora López', N'Av. Estrella Sadhalá #102, Santiago', '20261024', N'10:30', N'Carlos Méndez'),
                    (3, 1, N'Farmacia El Sol', N'Calle 16 #7, La Vega', '20261024', N'13:30', N'Luis Peña');

                DELETE visita
                FROM [Visitas] AS visita
                INNER JOIN [Solicitudes] AS solicitud ON solicitud.[Id] = visita.[SolicitudId]
                INNER JOIN @DatosSemilla AS semilla
                    ON semilla.[ClienteId] = solicitud.[ClienteId]
                    AND semilla.[ServicioId] = solicitud.[ServicioId]
                    AND semilla.[Contacto] = solicitud.[Contacto]
                    AND semilla.[Direccion] = solicitud.[Direccion]
                    AND semilla.[Fecha] = CONVERT(date, solicitud.[Fecha])
                    AND semilla.[Hora] = solicitud.[Hora]
                    AND semilla.[TecnicoAsignado] = solicitud.[TecnicoAsignado]
                WHERE solicitud.[FechaSolicitud] = '20261014';

                DELETE solicitud
                FROM [Solicitudes] AS solicitud
                INNER JOIN @DatosSemilla AS semilla
                    ON semilla.[ClienteId] = solicitud.[ClienteId]
                    AND semilla.[ServicioId] = solicitud.[ServicioId]
                    AND semilla.[Contacto] = solicitud.[Contacto]
                    AND semilla.[Direccion] = solicitud.[Direccion]
                    AND semilla.[Fecha] = CONVERT(date, solicitud.[Fecha])
                    AND semilla.[Hora] = solicitud.[Hora]
                    AND semilla.[TecnicoAsignado] = solicitud.[TecnicoAsignado]
                WHERE solicitud.[FechaSolicitud] = '20261014';

                DELETE usuario
                FROM [Usuarios] AS usuario
                WHERE usuario.[UsuarioLogin] = N'carlos'
                    AND usuario.[Nombre] = N'Carlos Méndez'
                    AND usuario.[PasswordHash] = N'AQAAAAIAAYagAAAAEMB/vjFzmu7a6+6obTRsJEzOzWZkBqfG9Sy1qVmJ/d1f0CP61CbwqUdTOQM0lzd+pQ=='
                    AND NOT EXISTS (SELECT 1 FROM [Visitas] WHERE [TecnicoId] = usuario.[Id]);
                """);
        }
    }
}
