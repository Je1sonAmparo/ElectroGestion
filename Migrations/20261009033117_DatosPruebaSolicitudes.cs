using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ElectroGestion.Migrations
{
    /// <inheritdoc />
    public partial class DatosPruebaSolicitudes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Solicitudes",
                columns: new[] { "Id", "ClienteId", "Contacto", "Direccion", "EstadoSolicitudId", "Fecha", "FechaFin", "FechaSolicitud", "Hora", "ServicioId", "TecnicoAsignado" },
                values: new object[,]
                {
                    { 1, 1, "Melody Santos", "Calle Duarte #24, Santiago", 1, new DateTime(2026, 10, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "09:00", 2, "" },
                    { 2, 2, "Constructora López", "Av. Estrella Sadhalá #102, Santiago", 2, new DateTime(2026, 10, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new DateTime(2026, 10, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), "10:30", 1, "" },
                    { 3, 3, "Farmacia El Sol", "Calle 16 #7, La Vega", 1, new DateTime(2026, 10, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new DateTime(2026, 10, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "14:00", 3, "" },
                    { 4, 4, "Colmado Hernández", "Calle Principal #58, Moca", 3, new DateTime(2026, 10, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new DateTime(2026, 9, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), "08:30", 4, "" },
                    { 5, 5, "Taller Mecánico RD", "Carretera Luperón km 5, Santiago", 2, new DateTime(2026, 10, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new DateTime(2026, 10, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "11:00", 5, "" },
                    { 6, 1, "Melody Santos", "Calle Duarte #24, Santiago", 3, new DateTime(2026, 9, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new DateTime(2026, 9, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "15:00", 3, "" },
                    { 7, 2, "Constructora López", "Av. Estrella Sadhalá #102, Santiago", 4, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new DateTime(2026, 9, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "09:30", 4, "" },
                    { 8, 3, "Farmacia El Sol", "Calle 16 #7, La Vega", 1, new DateTime(2026, 10, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new DateTime(2026, 10, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), "10:00", 2, "" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Solicitudes",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Solicitudes",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Solicitudes",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Solicitudes",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Solicitudes",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Solicitudes",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Solicitudes",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Solicitudes",
                keyColumn: "Id",
                keyValue: 8);
        }
    }
}
