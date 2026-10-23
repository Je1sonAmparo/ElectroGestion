using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ElectroGestion.Migrations
{
    /// <inheritdoc />
    public partial class AgregarLevantamientosYEstadoAdministracion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Levantamientos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SolicitudId = table.Column<int>(type: "int", nullable: false),
                    DisponibilidadEspacios = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    CargasElectricas = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    EstabilidadTension = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    RecorridoTuberias = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    RecorridoCableado = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    FotografiasJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BorradorGuardado = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Levantamientos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Levantamientos_Solicitudes_SolicitudId",
                        column: x => x.SolicitudId,
                        principalTable: "Solicitudes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "EstadosSolicitud",
                columns: new[] { "Id", "Nombre" },
                values: new object[] { 5, "En Espera Administración" });

            migrationBuilder.CreateIndex(
                name: "IX_Levantamientos_SolicitudId",
                table: "Levantamientos",
                column: "SolicitudId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Levantamientos");

            migrationBuilder.DeleteData(
                table: "EstadosSolicitud",
                keyColumn: "Id",
                keyValue: 5);
        }
    }
}
