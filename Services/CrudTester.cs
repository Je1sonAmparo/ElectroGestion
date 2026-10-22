using ElectroGestion.Models;

namespace ElectroGestion.Services;

public static class CrudTester
{
    public static async Task<bool> RunAsync(ClienteService clienteService, ServicioService servicioService, SolicitudService solicitudService, VisitaService visitaService)
    {
        Console.WriteLine("========================================");
        Console.WriteLine("INICIANDO PRUEBAS DE CHOQUE DE HORARIOS Y VISITAS");
        Console.WriteLine("========================================");

        var tecnicos = await visitaService.ObtenerTecnicosAsync();
        if (tecnicos.Count < 2)
        {
            Console.WriteLine("❌ ERROR: Se requieren al menos 2 técnicos para la prueba de choque.");
            return false;
        }

        var tecCarlos = tecnicos[0];
        var tecLuis = tecnicos[1];
        Console.WriteLine($"Técnicos identificados: {tecCarlos.Nombre} (ID {tecCarlos.Id}) y {tecLuis.Nombre} (ID {tecLuis.Id})");

        var clientesExistentes = await clienteService.GetList(c => true);
        var serviciosExistentes = await servicioService.GetList(s => true);

        var clienteTest = clientesExistentes.First();
        var servicioTest = serviciosExistentes.First();

        var solA = new Solicitud
        {
            ClienteId = clienteTest.Id,
            ServicioId = servicioTest.Id,
            Contacto = "Contacto Demo A",
            Direccion = "Calle 16 #10, Santiago",
            FechaSolicitud = DateTime.Today,
            EstadoSolicitudId = 1 // Pendiente
        };
        await solicitudService.Guardar(solA);

        var solB = new Solicitud
        {
            ClienteId = clienteTest.Id,
            ServicioId = servicioTest.Id,
            Contacto = "Contacto Demo B",
            Direccion = "Av. Las Carreras #20, Santiago",
            FechaSolicitud = DateTime.Today,
            EstadoSolicitudId = 1 // Pendiente
        };
        await solicitudService.Guardar(solB);

        var fechaPrueba = DateTime.Today.AddDays(1);
        while (fechaPrueba.DayOfWeek == DayOfWeek.Sunday)
        {
            fechaPrueba = fechaPrueba.AddDays(1);
        }

        Console.WriteLine($"Fecha de prueba laboral: {fechaPrueba:yyyy-MM-dd}");

        // 1. Agendar visita A para Carlos de 16:20 a 17:20
        var visitaA = new Visita
        {
            SolicitudId = solA.Id,
            TecnicoId = tecCarlos.Id,
            Fecha = fechaPrueba,
            Hora = "16:20",
            HoraFin = "17:20",
            Estado = "En proceso"
        };

        bool dispA = await visitaService.HorarioDisponibleAsync(fechaPrueba, "16:20", "17:20", null, tecCarlos.Id);
        if (!dispA)
        {
            Console.WriteLine("❌ ERROR: El horario inicial debería estar disponible para Carlos.");
            return false;
        }

        bool guardadoA = await visitaService.GuardarAsync(visitaA);
        if (!guardadoA)
        {
            Console.WriteLine("❌ ERROR: No se pudo guardar la visita de Carlos.");
            return false;
        }
        Console.WriteLine($"✅ Visita A para {tecCarlos.Nombre} (16:20 - 17:20) guardada exitosamente.");

        // Verificar que Solicitud A cambió a 'En proceso' (Id = 2)
        var solAVerif = await solicitudService.Buscar(solA.Id);
        if (solAVerif?.EstadoSolicitudId != 2)
        {
            Console.WriteLine($"❌ ERROR: EstadoSolicitudId de Solicitud A es {solAVerif?.EstadoSolicitudId}, se esperaba 2 ('En proceso').");
            return false;
        }
        Console.WriteLine("✅ Solicitud A pasó exitosamente al estado 'En proceso'.");

        // 2. Intentar agendar al MISMO técnico Carlos a la MISMA hora (16:20 a 17:20) -> DEBE DETECTAR COLISIÓN
        bool dispMismoTec = await visitaService.HorarioDisponibleAsync(fechaPrueba, "16:20", "17:20", null, tecCarlos.Id);
        if (dispMismoTec)
        {
            Console.WriteLine("❌ ERROR: El sistema debió detectar choque de horario para Carlos.");
            return false;
        }
        Console.WriteLine($"✅ Choque de horario detectado correctamente para el mismo técnico ({tecCarlos.Nombre}) a la misma hora.");

        // 3. Agendar para un SEGUNDO técnico (Luis) a la EXACTA MISMA HORA (16:20 a 17:20) -> DEBE ESTAR DISPONIBLE
        bool dispSegundoTec = await visitaService.HorarioDisponibleAsync(fechaPrueba, "16:20", "17:20", null, tecLuis.Id);
        if (!dispSegundoTec)
        {
            Console.WriteLine($"❌ ERROR: {tecLuis.Nombre} debería estar libre pero fue bloqueado por la cita de {tecCarlos.Nombre}.");
            return false;
        }
        Console.WriteLine($"✅ Horario DISPONIBLE para {tecLuis.Nombre} a las 16:20 a pesar de que {tecCarlos.Nombre} está ocupado.");

        var visitaB = new Visita
        {
            SolicitudId = solB.Id,
            TecnicoId = tecLuis.Id,
            Fecha = fechaPrueba,
            Hora = "16:20",
            HoraFin = "17:20",
            Estado = "En proceso"
        };

        bool guardadoB = await visitaService.GuardarAsync(visitaB);
        if (!guardadoB)
        {
            Console.WriteLine($"❌ ERROR: GuardarAsync falló al guardar la visita simultánea para {tecLuis.Nombre}.");
            return false;
        }
        Console.WriteLine($"✅ Visita B para {tecLuis.Nombre} (16:20 - 17:20) guardada exitosamente (múltiples técnicos concurrentes soportados).");

        // Verificar que Solicitud B cambió a 'En proceso' (Id = 2)
        var solBVerif = await solicitudService.Buscar(solB.Id);
        if (solBVerif?.EstadoSolicitudId != 2)
        {
            Console.WriteLine($"❌ ERROR: EstadoSolicitudId de Solicitud B es {solBVerif?.EstadoSolicitudId}, se esperaba 2 ('En proceso').");
            return false;
        }
        Console.WriteLine("✅ Solicitud B pasó exitosamente al estado 'En proceso'.");

        // Limpieza de datos de prueba
        await visitaService.EliminarAsync(visitaA.Id);
        await visitaService.EliminarAsync(visitaB.Id);
        await solicitudService.Eliminar(solA.Id);
        await solicitudService.Eliminar(solB.Id);
        Console.WriteLine("✅ Limpieza de pruebas finalizada.");

        Console.WriteLine("\n========================================");
        Console.WriteLine("🎉 TODAS LAS PRUEBAS DE CHOQUE DE HORARIOS Y VISITAS PASARON CON ÉXITO");
        Console.WriteLine("========================================");
        return true;
    }
}
