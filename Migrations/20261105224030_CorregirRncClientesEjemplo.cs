using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ElectroGestion.Migrations
{
    /// <inheritdoc />
    public partial class CorregirRncClientesEjemplo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE Clientes SET Rnc = '101-98765-2' WHERE Id = 2 AND Rnc = '101-987654-2'");
            migrationBuilder.Sql("UPDATE Clientes SET Rnc = '130-33445-3' WHERE Id = 3 AND Rnc = '130-334455-3'");
            migrationBuilder.Sql("UPDATE Clientes SET Rnc = '402-6677884-1' WHERE Id = 4 AND Rnc = '402-667788-4'");
            migrationBuilder.Sql("UPDATE Clientes SET Rnc = '130-11223-5' WHERE Id = 5 AND Rnc = '130-112233-5'");
            migrationBuilder.Sql("UPDATE Ventas SET DocumentoCliente = '101987652' WHERE Estado = N'Borrador' AND ClienteId = 2 AND DocumentoCliente = '1019876542'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE Clientes SET Rnc = '101-987654-2' WHERE Id = 2 AND Rnc = '101-98765-2'");
            migrationBuilder.Sql("UPDATE Clientes SET Rnc = '130-334455-3' WHERE Id = 3 AND Rnc = '130-33445-3'");
            migrationBuilder.Sql("UPDATE Clientes SET Rnc = '402-667788-4' WHERE Id = 4 AND Rnc = '402-6677884-1'");
            migrationBuilder.Sql("UPDATE Clientes SET Rnc = '130-112233-5' WHERE Id = 5 AND Rnc = '130-11223-5'");
        }
    }
}
