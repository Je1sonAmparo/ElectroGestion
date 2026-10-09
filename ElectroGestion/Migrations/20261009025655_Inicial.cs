using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ElectroGestion.Migrations
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Clientes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Correo = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Telefono2 = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Rnc = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Direccion = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Servicios = table.Column<int>(type: "int", nullable: false),
                    Ultimo = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clientes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EstadosSolicitud",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstadosSolicitud", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Servicios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServicioId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Categoria = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    PrecioBase = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Servicios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ClientesDirecciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    Direccion = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientesDirecciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientesDirecciones_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Solicitudes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    ServicioId = table.Column<int>(type: "int", nullable: false),
                    Contacto = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Direccion = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    FechaSolicitud = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Hora = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    EstadoSolicitudId = table.Column<int>(type: "int", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TecnicoAsignado = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Solicitudes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Solicitudes_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Solicitudes_EstadosSolicitud_EstadoSolicitudId",
                        column: x => x.EstadoSolicitudId,
                        principalTable: "EstadosSolicitud",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Solicitudes_Servicios_ServicioId",
                        column: x => x.ServicioId,
                        principalTable: "Servicios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DetallesSolicitud",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SolicitudId = table.Column<int>(type: "int", nullable: false),
                    ServicioId = table.Column<int>(type: "int", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Cantidad = table.Column<int>(type: "int", nullable: false),
                    Precio = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetallesSolicitud", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetallesSolicitud_Servicios_ServicioId",
                        column: x => x.ServicioId,
                        principalTable: "Servicios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DetallesSolicitud_Solicitudes_SolicitudId",
                        column: x => x.SolicitudId,
                        principalTable: "Solicitudes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Clientes",
                columns: new[] { "Id", "Correo", "Direccion", "Nombre", "Rnc", "Servicios", "Telefono", "Telefono2", "Ultimo" },
                values: new object[,]
                {
                    { 1, "melody@email.com", "Calle Duarte #24, Santiago", "Melody Santos", "402-1234567-1", 8, "809-000-0000", "829-111-2222", "Instalación de Sistema UPS" },
                    { 2, "info@clopez.com", "Av. Estrella Sadhalá #102, Santiago", "Constructora López", "101-987654-2", 5, "809-555-1212", "", "Instalación Solar" },
                    { 3, "farmacia@elsol.do", "Calle 16 #7, La Vega", "Farmacia El Sol", "130-334455-3", 6, "829-441-8890", "", "Mantenimiento Eléctrico" },
                    { 4, "hernandez@colmado.com", "Calle Principal #58, Moca", "Colmado Hernández", "402-667788-4", 3, "849-220-3311", "", "Cableado Estructurado" },
                    { 5, "taller@mecanicord.com", "Carretera Luperón km 5, Santiago", "Taller Mecánico RD", "130-112233-5", 1, "809-778-4402", "809-999-0000", "Instalación de Planta Eléctrica" }
                });

            migrationBuilder.InsertData(
                table: "EstadosSolicitud",
                columns: new[] { "Id", "Nombre" },
                values: new object[,]
                {
                    { 1, "Pendiente" },
                    { 2, "En proceso" },
                    { 3, "Finalizada" },
                    { 4, "Cancelada" }
                });

            migrationBuilder.InsertData(
                table: "Servicios",
                columns: new[] { "Id", "Categoria", "Estado", "Nombre", "PrecioBase", "ServicioId" },
                values: new object[,]
                {
                    { 1, "Instalaciones", "Activo", "Instalación de Paneles Solares", 85000m, "SV-001" },
                    { 2, "Instalaciones", "Activo", "Instalación de Sistema UPS", 45000m, "SV-002" },
                    { 3, "Mantenimiento", "Activo", "Mantenimiento Eléctrico", 12500m, "SV-003" },
                    { 4, "Redes y Cableado", "Activo", "Cableado Estructurado", 28000m, "SV-004" },
                    { 5, "Instalaciones", "Activo", "Instalación de Planta Eléctrica", 120000m, "SV-005" }
                });

            migrationBuilder.InsertData(
                table: "ClientesDirecciones",
                columns: new[] { "Id", "ClienteId", "Direccion", "Tipo" },
                values: new object[,]
                {
                    { 1, 1, "Calle Duarte #24, Santiago", "Principal" },
                    { 2, 2, "Av. Estrella Sadhalá #102, Santiago", "Principal" },
                    { 3, 3, "Calle 16 #7, La Vega", "Principal" },
                    { 4, 4, "Calle Principal #58, Moca", "Principal" },
                    { 5, 5, "Carretera Luperón km 5, Santiago", "Principal" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClientesDirecciones_ClienteId",
                table: "ClientesDirecciones",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesSolicitud_ServicioId",
                table: "DetallesSolicitud",
                column: "ServicioId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesSolicitud_SolicitudId",
                table: "DetallesSolicitud",
                column: "SolicitudId");

            migrationBuilder.CreateIndex(
                name: "IX_Solicitudes_ClienteId",
                table: "Solicitudes",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Solicitudes_EstadoSolicitudId",
                table: "Solicitudes",
                column: "EstadoSolicitudId");

            migrationBuilder.CreateIndex(
                name: "IX_Solicitudes_ServicioId",
                table: "Solicitudes",
                column: "ServicioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClientesDirecciones");

            migrationBuilder.DropTable(
                name: "DetallesSolicitud");

            migrationBuilder.DropTable(
                name: "Solicitudes");

            migrationBuilder.DropTable(
                name: "Clientes");

            migrationBuilder.DropTable(
                name: "EstadosSolicitud");

            migrationBuilder.DropTable(
                name: "Servicios");
        }
    }
}
