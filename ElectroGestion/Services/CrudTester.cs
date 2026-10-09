using ElectroGestion.Models;

namespace ElectroGestion.Services;

public static class CrudTester
{
    public static async Task<bool> RunAsync(ClienteService clienteService, ServicioService servicioService, SolicitudService solicitudService)
    {
        Console.WriteLine("========================================");
        Console.WriteLine("INICIANDO PRUEBAS DE CRUD EN BASE DE DATOS");
        Console.WriteLine("========================================");

        // 1. CLIENTE CRUD
        Console.WriteLine("\n[1] Probando CRUD de Clientes...");
        var nuevoCliente = new Cliente
        {
            Nombre = "Cliente Test Automatizado",
            Correo = "test.crud@electrogestion.com",
            Rnc = "402-9999999-9",
            Direccion = "Av. Circunvalación Norte #50, Santiago",
            Servicios = 0
        };
        nuevoCliente.Telefonos.Add(new ClienteTelefono { Numero = "809-555-0199", Tipo = "Principal" });
        nuevoCliente.Telefonos.Add(new ClienteTelefono { Numero = "829-555-0188", Tipo = "Alternativo" });

        bool clienteGuardado = await clienteService.Guardar(nuevoCliente);
        if (!clienteGuardado || nuevoCliente.Id == 0)
        {
            Console.WriteLine("❌ ERROR: No se pudo crear el cliente.");
            return false;
        }
        Console.WriteLine($"✅ Cliente CREADO con ID: {nuevoCliente.Id}");

        var clienteLeido = await clienteService.Buscar(nuevoCliente.Id);
        if (clienteLeido == null || clienteLeido.Nombre != "Cliente Test Automatizado" || clienteLeido.Telefonos.Count != 2)
        {
            Console.WriteLine("❌ ERROR: No se pudo leer el cliente o faltan teléfonos.");
            return false;
        }
        Console.WriteLine($"✅ Cliente LEÍDO correctamente. Teléfonos: {clienteLeido.Telefonos.Count}");

        clienteLeido.Nombre = "Cliente Test Modificado";
        clienteLeido.Direccion = "Calle Sol #10, Santiago";
        clienteLeido.Telefonos.Clear();
        clienteLeido.Telefonos.Add(new ClienteTelefono { Numero = "849-555-7777", Tipo = "Principal" });

        bool clienteModificado = await clienteService.Guardar(clienteLeido);
        if (!clienteModificado)
        {
            Console.WriteLine("❌ ERROR: No se pudo modificar el cliente.");
            return false;
        }
        Console.WriteLine("✅ Cliente MODIFICADO correctamente.");

        var clienteVerif = await clienteService.Buscar(nuevoCliente.Id);
        if (clienteVerif?.Nombre != "Cliente Test Modificado" || clienteVerif?.Telefono != "849-555-7777")
        {
            Console.WriteLine("❌ ERROR: La modificación del cliente no persistió los cambios esperados.");
            return false;
        }
        Console.WriteLine("✅ Verificación de modificación de cliente EXITOSA.");

        // 2. SERVICIO CRUD
        Console.WriteLine("\n[2] Probando CRUD de Servicios...");
        var nuevoServicio = new Servicio
        {
            ServicioId = "SV-TEST",
            Nombre = "Servicio de Diagnóstico Test",
            Categoria = "Diagnóstico",
            PrecioBase = 3500.00m,
            Estado = "Activo"
        };

        bool servGuardado = await servicioService.Guardar(nuevoServicio);
        if (!servGuardado || nuevoServicio.Id == 0)
        {
            Console.WriteLine("❌ ERROR: No se pudo crear el servicio.");
            return false;
        }
        Console.WriteLine($"✅ Servicio CREADO con ID: {nuevoServicio.Id} (Código: {nuevoServicio.ServicioId})");

        var servLeido = await servicioService.Buscar(nuevoServicio.Id);
        if (servLeido == null || servLeido.Nombre != "Servicio de Diagnóstico Test")
        {
            Console.WriteLine("❌ ERROR: No se pudo leer el servicio.");
            return false;
        }
        Console.WriteLine("✅ Servicio LEÍDO correctamente.");

        servLeido.Nombre = "Servicio de Diagnóstico Avanzado";
        servLeido.PrecioBase = 4200.00m;
        bool servModificado = await servicioService.Guardar(servLeido);
        if (!servModificado)
        {
            Console.WriteLine("❌ ERROR: No se pudo modificar el servicio.");
            return false;
        }
        Console.WriteLine("✅ Servicio MODIFICADO correctamente.");

        var servVerif = await servicioService.Buscar(nuevoServicio.Id);
        if (servVerif?.PrecioBase != 4200.00m || servVerif?.Nombre != "Servicio de Diagnóstico Avanzado")
        {
            Console.WriteLine("❌ ERROR: La modificación del servicio no persistió los cambios.");
            return false;
        }
        Console.WriteLine("✅ Verificación de modificación de servicio EXITOSA.");

        // 3. SOLICITUD CRUD
        Console.WriteLine("\n[3] Probando CRUD de Solicitudes...");
        var nuevaSolicitud = new Solicitud
        {
            ClienteId = nuevoCliente.Id,
            ServicioId = nuevoServicio.Id,
            Contacto = nuevoCliente.Nombre,
            Direccion = "Av. Circunvalación Norte #50, Santiago",
            FechaSolicitud = DateTime.Today,
            Fecha = DateTime.Today.AddDays(3),
            Hora = "10:30",
            EstadoSolicitudId = 1
        };

        bool solGuardada = await solicitudService.Guardar(nuevaSolicitud);
        if (!solGuardada || nuevaSolicitud.Id == 0)
        {
            Console.WriteLine("❌ ERROR: No se pudo crear la solicitud.");
            return false;
        }
        Console.WriteLine($"✅ Solicitud CREADA con ID: {nuevaSolicitud.Id}");

        var solLeida = await solicitudService.Buscar(nuevaSolicitud.Id);
        if (solLeida == null || solLeida.Cliente?.Id != nuevoCliente.Id || solLeida.Servicio?.Id != nuevoServicio.Id)
        {
            Console.WriteLine("❌ ERROR: No se pudo leer la solicitud o sus relaciones asociadas.");
            return false;
        }
        Console.WriteLine($"✅ Solicitud LEÍDA correctamente. Cliente relacionado: {solLeida.Cliente?.Nombre}, Servicio: {solLeida.Servicio?.Nombre}");

        solLeida.Direccion = "Nueva dirección de visita #99";
        solLeida.Hora = "15:45";
        solLeida.EstadoSolicitudId = 2; // En proceso

        bool solModificada = await solicitudService.Guardar(solLeida);
        if (!solModificada)
        {
            Console.WriteLine("❌ ERROR: No se pudo modificar la solicitud.");
            return false;
        }
        Console.WriteLine("✅ Solicitud MODIFICADA correctamente.");

        var solVerif = await solicitudService.Buscar(nuevaSolicitud.Id);
        if (solVerif?.Direccion != "Nueva dirección de visita #99" || solVerif?.Hora != "15:45" || solVerif?.EstadoSolicitudId != 2)
        {
            Console.WriteLine("❌ ERROR: La modificación de la solicitud no persistió los cambios.");
            return false;
        }
        Console.WriteLine("✅ Verificación de modificación de solicitud EXITOSA.");

        // 4. INTEGRIDAD REFERENCIAL Y ELIMINACIÓN
        Console.WriteLine("\n[4] Probando Integridad Referencial y Eliminación...");
        
        // El cliente tiene solicitud activa, no debe poder eliminarse
        bool clienteEliminadoConSolicitud = await clienteService.Eliminar(nuevoCliente.Id);
        if (clienteEliminadoConSolicitud)
        {
            Console.WriteLine("❌ ERROR: El cliente no debió poder eliminarse teniendo una solicitud activa.");
            return false;
        }
        Console.WriteLine("✅ Protección FK de Cliente validada (bloqueado correctamente cuando tiene solicitudes).");

        // El servicio tiene solicitud activa, no debe poder eliminarse
        bool servEliminadoConSolicitud = await servicioService.Eliminar(nuevoServicio.Id);
        if (servEliminadoConSolicitud)
        {
            Console.WriteLine("❌ ERROR: El servicio no debió poder eliminarse teniendo una solicitud activa.");
            return false;
        }
        Console.WriteLine("✅ Protección FK de Servicio validada (bloqueado correctamente cuando tiene solicitudes).");

        // Eliminar la solicitud
        bool solEliminada = await solicitudService.Eliminar(nuevaSolicitud.Id);
        if (!solEliminada)
        {
            Console.WriteLine("❌ ERROR: No se pudo eliminar la solicitud.");
            return false;
        }
        Console.WriteLine("✅ Solicitud ELIMINADA exitosamente.");

        var solPostDelete = await solicitudService.Buscar(nuevaSolicitud.Id);
        if (solPostDelete != null)
        {
            Console.WriteLine("❌ ERROR: La solicitud sigue existiendo tras ser eliminada.");
            return false;
        }
        Console.WriteLine("✅ Verificación de eliminación de solicitud EXITOSA.");

        // Ahora que la solicitud fue eliminada, el cliente sí debe poder eliminarse
        bool clienteEliminadoFinal = await clienteService.Eliminar(nuevoCliente.Id);
        if (!clienteEliminadoFinal)
        {
            Console.WriteLine("❌ ERROR: No se pudo eliminar el cliente después de liberar su solicitud.");
            return false;
        }
        Console.WriteLine("✅ Cliente ELIMINADO exitosamente.");

        var clientePostDelete = await clienteService.Buscar(nuevoCliente.Id);
        if (clientePostDelete != null)
        {
            Console.WriteLine("❌ ERROR: El cliente sigue existiendo tras ser eliminado.");
            return false;
        }
        Console.WriteLine("✅ Verificación de eliminación de cliente EXITOSA.");

        // El servicio también debe poder eliminarse ahora
        bool servEliminadoFinal = await servicioService.Eliminar(nuevoServicio.Id);
        if (!servEliminadoFinal)
        {
            Console.WriteLine("❌ ERROR: No se pudo eliminar el servicio después de liberar su solicitud.");
            return false;
        }
        Console.WriteLine("✅ Servicio ELIMINADO exitosamente.");

        var servPostDelete = await servicioService.Buscar(nuevoServicio.Id);
        if (servPostDelete != null)
        {
            Console.WriteLine("❌ ERROR: El servicio sigue existiendo tras ser eliminado.");
            return false;
        }
        Console.WriteLine("✅ Verificación de eliminación de servicio EXITOSA.");

        Console.WriteLine("\n========================================");
        Console.WriteLine("🎉 TODAS LAS PRUEBAS DE CRUD PASARON CON ÉXITO");
        Console.WriteLine("========================================");
        return true;
    }
}
