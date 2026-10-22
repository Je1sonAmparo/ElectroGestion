# Manual de Usuario — ElectroGestión (versión 2)

Este manual está organizado por pantallas y tareas. Cada pantalla se explica una sola vez; cuando una acción o un campo cambia según el perfil, se incluye una nota en la sección correspondiente. En la aplicación el segundo perfil se denomina **Gerente**; en este manual también puede mencionarse como **Encargado**.

El menú lateral cambia según el perfil. La presencia o ausencia de una opción en el menú no implica por sí sola que la ruta esté protegida. La página `/usuarios` sí verifica el rol Gerente y redirige a Inicio cuando la sesión no corresponde a ese rol.

## 5. Manual de usuario

**5.1. Pantalla de Acceso al Sistema (`/login`)**

> Nota: Insertar captura de pantalla general de la pantalla de acceso.

Desde esta pantalla puede ingresar a ElectroGestión con las credenciales asignadas a su perfil.

**5.1.1. Interfaz principal**

> Nota: Insertar captura de pantalla del logotipo y formulario de inicio de sesión.

La pantalla muestra la identidad de ElectroGestión, una breve descripción del sistema y el formulario con los campos **Usuario** y **Contraseña**. La contraseña permanece oculta mientras se escribe.

**5.1.2. ¿Cómo iniciar sesión?**

1. Abra la pantalla de acceso. Los campos **Usuario** y **Contraseña** aparecerán vacíos.

> Nota: Insertar captura de pantalla del formulario de acceso vacío.

2. Escriba su nombre de usuario en **Usuario** y su contraseña en **Contraseña**.
3. Haga clic en **Iniciar sesión**. Si las credenciales son válidas, el sistema abrirá la pantalla de Inicio.

> Nota: Insertar captura de pantalla señalando el botón Iniciar sesión.

Si falta algún campo, las credenciales no coinciden o no es posible conectar con el servicio de autenticación, el sistema mostrará un mensaje de validación.

**Credenciales de demostración documentadas en el README:**

- Técnico: `luis` / `luis` (Luis Peña) y `carlos` / `carlos` (Carlos Méndez).
- Gerente/Encargado: `ana` / `ana` (Ana Hernández).

**5.2. Navegación y controles generales**

> Nota: Insertar captura de pantalla del menú lateral expandido, el perfil y los controles persistentes.

El menú lateral permite acceder a las secciones disponibles. Las opciones compartidas incluyen **Inicio** (`/`), **Solicitudes** (`/solicitudes`), **Agenda** (`/agenda`), **Órdenes de trabajo** (`/ordenes`), **Clientes** (`/clientes`) e **Historial** (`/historial`).

Según el perfil, también se muestran estas opciones:

- **Técnico:** **Levantamientos** (`/levantamientos`).
- **Gerente:** **Cotizaciones** (`/cotizaciones`), **Cobros** (`/cobros`), **Servicios** (`/servicios`), **Inventario** (`/inventario`), **Proveedores** (`/proveedores`) y **Reportes** (`/reportes`).

La ruta `/usuarios` existe y verifica el rol Gerente, pero no aparece como elemento del menú lateral.

El logotipo lleva a Inicio. El botón junto al menú permite contraerlo o expandirlo. En la parte inferior se muestra la inicial, el nombre y el cargo o rol del usuario conectado.

**5.2.1. ¿Cómo cerrar sesión?**

1. Haga clic en **Cerrar sesión**.
2. En la confirmación, seleccione **Cancelar** para permanecer en el sistema o **Cerrar sesión** para salir.
3. Al confirmar, la aplicación cierra la sesión y vuelve a `/`.

> Nota: Insertar captura de pantalla del cuadro de confirmación Cerrar sesión.

**5.3. Pantalla de Inicio / Dashboard (`/`)**

> Nota: Insertar captura de pantalla general del Dashboard.

El Dashboard resume la actividad operativa y ofrece accesos a las áreas de trabajo.

**5.3.1. Indicadores operativos**

> Nota: Insertar captura de pantalla de los indicadores de la parte superior.

Los indicadores muestran las solicitudes pendientes, las visitas programadas, los levantamientos pendientes y las órdenes de trabajo nuevas. Los enlaces **Ver solicitudes**, **Ver agenda**, **Ver levantamientos** y **Ver órdenes** abren sus secciones correspondientes.

**5.3.2. Próxima visita**

> Nota: Insertar captura de pantalla del panel Próxima visita.

Si existe una visita programada para hoy, el panel muestra la hora, el cliente, el servicio, la dirección y una representación de ubicación. **Ver ubicación** abre la ubicación en el mapa. Si no hay visitas programadas para hoy, se muestra un mensaje informativo.

**5.3.3. Actividad reciente**

> Nota: Insertar captura de pantalla del panel Actividad reciente.

El panel presenta hasta tres actividades con el tipo de servicio, el cliente y la fecha de registro. **Ver todas las actividades** abre Solicitudes.

**5.3.4. Accesos rápidos**

> Nota: Insertar captura de pantalla de la sección Accesos rápidos.

Desde esta sección puede abrir Solicitudes, Agenda, Levantamientos, Órdenes de trabajo o Clientes. **Nueva solicitud** lleva al listado de Solicitudes; desde allí se abre el formulario con el botón correspondiente. **Crear visita** lleva a Agenda.

**Nota de perfil:** Los accesos rápidos pueden incluir Levantamientos aunque esa opción no aparezca en el menú del Gerente. El menú lateral y las tarjetas rápidas no presentan necesariamente las mismas opciones.

**5.4. Pantalla de Solicitudes (`/solicitudes`)**

> Nota: Insertar captura de pantalla general de Solicitudes.

En esta sección puede buscar, registrar, consultar y editar solicitudes de atención técnica.

**5.4.1. Filtros y tabla**

> Nota: Insertar captura de pantalla de los filtros y la tabla de solicitudes.

Puede localizar registros por ID, cliente, tipo de servicio, fecha de registro de la solicitud o estado. Los estados disponibles son **Todos los estados**, **Pendiente**, **En proceso**, **Finalizada** y **Cancelada**. Después de seleccionar los criterios, haga clic en **Buscar**.

La tabla muestra ID, cliente, tipo de servicio, fecha de solicitud, estado y acciones. Los encabezados de ID, cliente, servicio, fecha y estado permiten ordenar los resultados. La fecha corresponde al registro de la solicitud; la programación de visitas se gestiona en Agenda.

Las acciones comunes por registro son **Ver detalle** y **Editar**.

**5.4.2. ¿Cómo registrar una nueva solicitud?**

1. Haga clic en **Nueva solicitud**.

> Nota: Insertar captura de pantalla señalando el botón Nueva solicitud.

2. El sistema abrirá la ventana **Nueva Solicitud** con los campos vacíos.

> Nota: Insertar captura de pantalla de la ventana Nueva Solicitud vacía.

3. Complete los datos:

- **Cliente:** seleccione un cliente registrado.
- **Tipo de servicio:** seleccione el servicio solicitado.
- **Dirección del servicio:** escriba la ubicación donde se requiere la atención.

4. Haga clic en **Guardar** para registrar la solicitud o en **Cancelar** para cerrar la ventana sin guardarla. La fecha y el horario de la visita se programan después desde Agenda.

> Nota: Insertar captura de pantalla de Nueva Solicitud con los datos completos y otra señalando Guardar.

**5.4.3. ¿Cómo consultar una solicitud?**

1. En la tabla, localice la solicitud y haga clic en el icono **Ver detalle**.
2. La ventana **Detalle de la solicitud** muestra el ID, cliente, contacto, tipo de servicio, dirección, fechas disponibles, horario y estado.
3. Para regresar a la tabla, haga clic en **Cerrar** o en el icono X.

> Nota: Insertar capturas de pantalla del icono Ver detalle y de la ventana Detalle de la solicitud.

**5.4.4. ¿Cómo editar una solicitud?**

1. En la tabla, localice la solicitud y haga clic en el icono **Editar**.
2. El sistema abrirá **Editar Solicitud** con los datos actuales.

> Nota: Insertar captura de pantalla de Editar Solicitud con los datos cargados.

3. Revise o actualice los campos habilitados, como el tipo de servicio y la dirección. El campo de cliente puede aparecer bloqueado durante la edición.
4. Haga clic en **Guardar** para aplicar los cambios o en **Cancelar** para salir sin guardarlos. La programación de la visita se gestiona desde Agenda.

> Nota: Insertar captura de pantalla de Editar Solicitud con cambios y otra señalando Guardar.

**5.4.5. ¿Cómo eliminar una solicitud?**

1. En las acciones del registro, seleccione **Eliminar**.
2. En la confirmación **Eliminar Solicitud**, revise el aviso de que la operación no se puede deshacer.
3. Seleccione **Eliminar** para confirmar o **Cancelar** para conservar la solicitud.

> Nota: Insertar capturas de pantalla de la acción Eliminar y del cuadro de confirmación.

**Nota de perfil:** La acción para eliminar una solicitud está disponible únicamente para el Gerente. El Técnico puede consultar y editar las solicitudes según las acciones que aparecen en su pantalla. La solicitud eliminada deja de aparecer en las listas; la operación no elimina físicamente los datos conservados por el sistema.

**5.5. Pantalla de Agenda (`/agenda`)**

> Nota: Insertar captura de pantalla general de la vista de Agenda.

En esta sección puede revisar la agenda y programar o gestionar visitas técnicas.

**5.5.1. Interfaz principal**

**Controles de vista y navegación**

> Nota: Insertar captura de pantalla de los selectores Hoy, Semanal, Mensual, técnico y navegación.

Puede cambiar la vista del calendario entre **Hoy**, **Semanal** y **Mensual**. Cuando está disponible, también puede filtrar por todos los técnicos o por uno específico. Las flechas permiten avanzar o retroceder un día, una semana o un mes, según la vista. En la vista diaria, el cambio de fecha omite los domingos.

**Calendario y panel de detalle**

> Nota: Insertar captura de pantalla del calendario con visitas y del panel de detalle.

Las vistas semanal y mensual muestran los días de lunes a sábado. En la vista **Hoy**, si se selecciona un domingo, el sistema informa que la agenda no está disponible. Al seleccionar una visita se muestran cliente, servicio, fecha, horas, dirección, técnico asignado y estado. **Ver ubicación** aparece en el detalle, aunque actualmente no tiene una acción asociada.

**5.5.2. ¿Cómo programar una nueva visita?**

1. Haga clic en **+ Nueva visita**.
2. El sistema abrirá la ventana **Nueva visita** con el formulario vacío.

> Nota: Insertar capturas de pantalla del botón y de la ventana Nueva visita.

3. Complete los campos:

- **Solicitud:** seleccione una solicitud que todavía no tenga una visita asignada.
- **Técnico asignado:** se asigna por defecto al Técnico de la sesión; el Gerente puede elegir un técnico o **Sin asignar**.
- **Dirección:** se completa desde la solicitud y es de solo lectura.
- **Fecha:** elija un día de lunes a sábado; no se programan visitas en domingo.
- **Hora de inicio y hora de fin:** ambas deben estar entre las 08:00 a. m. y las 06:00 p. m.; la hora de fin debe ser posterior a la de inicio.

El sistema comprueba que la solicitud esté disponible y que el horario no se superponga con otra visita de esa fecha.

4. Si selecciona domingo, revise el aviso; **Guardar visita** permanecerá deshabilitado.
5. Haga clic en **Guardar visita** para registrar la visita o en **Cancelar** para cerrar el formulario.

> Nota: Insertar captura de pantalla de Nueva visita con los campos completos y otra señalando Guardar visita.

**5.5.3. ¿Cómo editar una visita programada?**

1. Seleccione la visita en el calendario y haga clic en **Editar visita** en el panel de detalle.
2. El sistema abrirá **Editar visita** con los datos actuales.
3. Revise los campos disponibles:

- **Solicitud:** seleccione la solicitud correspondiente.
- **Técnico asignado:** el Técnico ve su asignación asociada a la sesión; el Gerente puede seleccionar un técnico o **Sin asignar**.
- **Dirección:** se completa desde la solicitud y es de solo lectura.
- **Fecha y horas:** elija un día de lunes a sábado y un intervalo entre las 08:00 a. m. y las 06:00 p. m.; la hora final debe ser posterior a la inicial.

4. El sistema valida la fecha, el intervalo y las superposiciones; no cuenta la visita que está editando como conflicto.
5. Haga clic en **Guardar cambios** para actualizar la agenda o en **Cancelar** para descartar la edición.

> Nota: Insertar capturas de pantalla de Editar visita y del botón Guardar cambios.

**5.5.4. ¿Cómo eliminar una visita?**

1. Abra el formulario de edición de la visita y haga clic en **Eliminar visita**.
2. En la confirmación, haga clic en **Sí, eliminar** para quitarla de la agenda o en **Cancelar** para mantenerla.

> Nota: Insertar capturas de pantalla de Eliminar visita y del cuadro de confirmación.

La visita deja de aparecer en la agenda; la solicitud asociada permanece registrada y disponible.

**5.5.5. ¿Cómo cancelar o reactivar una visita?**

1. Seleccione una visita activa en el calendario y elija **Cancelar visita**.
2. En la confirmación, haga clic en **Sí, cancelar**. El estado pasará a **Cancelada**.
3. Para reactivarla, seleccione la visita cancelada, elija **Reactivar visita** y confirme con **Sí, reactivar**. El estado volverá a **En proceso**.

> Nota: Insertar capturas de pantalla de las confirmaciones Cancelar visita y Reactivar visita.

**5.6. Pantalla de Levantamientos (`/levantamientos`) — uso del Técnico**

> Nota: Insertar captura de pantalla general de la vista de Levantamientos.

En esta sección puede consultar y completar los levantamientos técnicos relacionados con solicitudes.

**5.6.1. Interfaz principal**

> Nota: Insertar captura de pantalla de los filtros y tarjetas de levantamientos.

Los filtros permiten buscar por ID, cliente, tipo de servicio, fecha y estado. Entre los estados visibles se encuentran **En Espera Técnico**, **En Espera Administración**, **Aprobado** y **Borrador**. Las tarjetas muestran estado, cliente, servicio y fecha programada. Solo aparecen solicitudes con fecha y hora definidas.

En una tarjeta, **Comenzar** abre los levantamientos **En Espera Técnico**; para otros estados disponibles se muestra **Ver detalle**.

**5.6.2. ¿Cómo completar o consultar un levantamiento?**

1. En la tarjeta correspondiente, haga clic en **Comenzar** o **Ver detalle**. La página cambiará al formulario; no se abre una ventana emergente.
2. Complete o revise los datos técnicos:

- **Cliente** y **Tipo de servicio:** seleccione los valores correspondientes. Si el levantamiento se abrió desde una solicitud, estos campos quedan bloqueados y se completan desde ella.
- **Disponibilidad de los espacios físicos.**
- **Cargas eléctricas.**
- **Estabilidad de la tensión.**
- **Recorrido físico de las tuberías.**
- **Recorrido físico del cableado.**
- **Observaciones.**

Los campos señalados con asterisco (*) aparecen como requeridos en la pantalla.

> Nota: Insertar captura de pantalla del formulario con los datos técnicos.

3. En la sección de fotografías, haga clic en **+ Agregar foto**. Para quitar una fotografía incorporada, use la X de su miniatura.
4. Si abrió el levantamiento desde una solicitud, revise el aviso de origen; **Volver a levantamientos** regresa al listado.
5. Elija la acción correspondiente:

- **Guardar:** conserva el levantamiento como borrador para retomarlo desde la lista.
- **Enviar a Administración:** envía el levantamiento para su revisión.
- **Cancelar servicio:** cancela el servicio desde el formulario.
- **Cancelar:** vuelve al listado sin continuar en el formulario.

> Nota: Insertar capturas de pantalla de las fotografías y de los botones de acción.

**5.7. Pantalla de Cotizaciones (`/cotizaciones`) — uso del Gerente**

> Nota: Insertar captura de pantalla general de Cotizaciones.

Esta pantalla permite consultar y mantener las cotizaciones registradas.

**5.7.1. Filtros y tabla**

> Nota: Insertar captura de pantalla de filtros y tabla.

Filtre por ID, solicitud, cliente y estado (**Todos**, **Pendiente**, **Aprobada** o **Rechazada**). La tabla muestra ID, solicitud, cliente, fecha, monto, estado y acciones de consulta o edición.

**5.7.2. ¿Cómo crear una cotización?**

1. Haga clic en **Nueva Cotización**.
2. En la ventana **Nueva Cotización**, complete:

- **ID Solicitud:** escriba el identificador de la solicitud.
- **Cliente:** seleccione o indique el cliente según las opciones disponibles.
- **Monto Total:** introduzca un monto mayor que cero; admite decimales.

3. Revise la información y haga clic en **Guardar**. **Cancelar** cierra la ventana sin guardar.

> Nota: Insertar capturas de pantalla de Nueva Cotización vacía y con los datos completos.

El sistema requiere los datos indicados y valida el cliente de acuerdo con el registro disponible.

**5.7.3. ¿Cómo editar o consultar una cotización?**

1. Haga clic en **Editar** en la cotización que desea modificar.
2. En **Editar Cotización**, revise o actualice los campos:

- **ID Solicitud:** puede modificarse.
- **Cliente:** aparece bloqueado durante la edición.
- **Monto Total:** debe ser mayor que cero y puede incluir decimales.
- **Estado:** seleccione **Pendiente**, **Aprobada** o **Rechazada**.

3. Haga clic en **Guardar** para aplicar los cambios.
4. Para consultar, haga clic en el icono de vista y revise solicitud, cliente, fecha, monto total y estado en **Detalle de Cotización**. Cierre con **Cerrar** o la X.

> Nota: Insertar capturas de pantalla de Editar Cotización y Detalle de Cotización.

No se encontró una acción de eliminación para las cotizaciones.

**5.8. Pantalla de Órdenes de Trabajo (`/ordenes`)**

> Nota: Insertar captura de pantalla general de Órdenes de Trabajo.

Esta sección presenta las órdenes de trabajo y las acciones disponibles para completar o administrar su ejecución.

**5.8.1. Consultar las órdenes**

> Nota: Insertar captura de pantalla de filtros y una tarjeta de orden.

Los filtros disponibles incluyen ID, cliente, tipo de servicio, fecha y estado (**Todas**, **En proceso** o **Finalizada**). Las tarjetas presentan el código o ID, cliente, servicio, estado, checklist técnico, materiales y herramientas asignadas e instrucciones de campo cuando existen.

**5.8.2. Completar el trabajo asignado**

1. Revise las tareas, materiales, herramientas e instrucciones indicadas en la orden.
2. Al terminar el trabajo de campo, haga clic en **Completar Trabajo en Campo**.

El resultado visible es la orden en estado **Finalizada** y la solicitud asociada actualizada a **Finalizada**. En las órdenes finalizadas se muestra **Orden Finalizada** en lugar del botón.

> Nota: Insertar captura de pantalla señalando Completar Trabajo en Campo y del estado finalizado.

**5.8.3. Crear una orden**

1. En la vista de administración, haga clic en **+ Nueva Orden de Trabajo**.
2. En la ventana **Nueva Orden de Trabajo**, complete:

- **Cotización aprobada:** seleccione una cotización aprobada; es necesaria para crear la orden.
- **Cliente asignado:** se completa desde la cotización y es de solo lectura.
- **Tipo de servicio:** indique el servicio que se realizará.
- **Checklist de tareas:** escriba una tarea por línea.
- **Materiales y herramientas:** separe los elementos con comas.
- **Observaciones / Instrucciones:** agregue notas para el equipo de campo si son necesarias.

3. Revise la información y haga clic en **Crear Orden**. **Cancelar** cierra el formulario. Si no selecciona una cotización aprobada, el sistema muestra un aviso y no crea la orden.

> Nota: Insertar capturas de pantalla de Nueva Orden de Trabajo y sus campos completados.

**5.8.4. Editar una orden**

1. Haga clic en **Editar** en la tarjeta correspondiente.
2. Revise los datos cargados y actualice tipo de servicio, checklist, materiales y herramientas u observaciones.
3. El cliente asignado es de solo lectura. El selector de cotización aprobada permanece visible, pero no se precarga con una cotización asociada.
4. Haga clic en **Crear Orden** para guardar los cambios.

> Nota: Insertar capturas de pantalla del formulario de edición y del botón Crear Orden.

**5.8.5. Eliminar una orden o cambiar su estado**

- Para eliminar, use la acción correspondiente de la tarjeta y confirme en **Eliminar Orden**.
- Para cambiar el estado, use la acción de estado de la tarjeta; alterna entre **En proceso** y **Finalizada**.

> Nota: Insertar capturas de pantalla de las acciones Editar, Eliminar y Estado.

**Nota de perfil:** En la vista del Técnico se presentan las órdenes asignadas y la acción para completar el trabajo de campo. La vista del Gerente incluye las acciones administrativas descritas en esta sección, como crear, editar, eliminar y cambiar el estado.

**5.9. Pantalla de Clientes (`/clientes`)**

> Nota: Insertar captura de pantalla general de Clientes.

La pantalla permite buscar, consultar, registrar y editar clientes. La eliminación aparece únicamente en la vista del Gerente.

**5.9.1. Filtros y tabla**

> Nota: Insertar captura de pantalla de los filtros y la tabla.

Puede filtrar por ID, nombre, RNC/Cédula, correo, teléfono y dirección. La tabla muestra cliente, teléfono, dirección, número de servicios y acciones. Los encabezados de cliente, teléfono, dirección y servicios permiten ordenar. Las acciones comunes de fila son ver detalle y editar.

**5.9.2. ¿Cómo registrar o editar un cliente?**

1. Haga clic en **Nuevo cliente** para registrar uno o en **Editar** en la fila correspondiente para modificarlo.
2. Se abrirá **Nuevo cliente** vacío o **Editar cliente** con los datos actuales.
3. Complete o revise los datos:

- **Nombre o razón social**, **correo electrónico**, **teléfono principal**, **RNC/Cédula** y **dirección** son obligatorios.
- **Teléfono alternativo** es opcional.

4. Haga clic en **Guardar cambios**. Si falta un campo obligatorio, el sistema muestra un aviso. **Cancelar** cierra el formulario.

> Nota: Insertar capturas de pantalla de los formularios de alta y edición, y del botón Guardar cambios.

**5.9.3. ¿Cómo consultar un cliente?**

1. Use el icono de vista para abrir el detalle.
2. Revise el nombre, correo, teléfonos principal y alternativo, RNC/Cédula, dirección, servicios realizados y último servicio.
3. Cierre con **Cerrar** o la X.

> Nota: Insertar captura de pantalla del detalle del cliente.

**5.9.4. ¿Cómo eliminar un cliente?**

1. En la vista del Gerente, abra **Editar cliente** y haga clic en **Eliminar**.
2. En la confirmación **Eliminar Cliente**, seleccione **Eliminar** para confirmar o **Cancelar** para salir.

La operación puede ser rechazada si el cliente tiene solicitudes asociadas.

> Nota: Insertar capturas de pantalla de la acción y confirmación de eliminación.

**Nota de perfil:** La acción de eliminación está disponible para el Gerente; no se muestra en la vista del Técnico.

**5.10. Pantalla de Cobros (`/cobros`) — uso del Gerente**

> Nota: Insertar captura de pantalla general de Cobros.

Esta sección permite registrar, consultar y actualizar cobros.

**5.10.1. Filtros y tabla**

Filtre por ID, cliente, Factura ID y estado (**Todos**, **Pendiente**, **Completado** o **Cancelado**). La tabla presenta ID, cliente, factura, fecha, método de pago, monto, estado y acciones de vista o edición.

> Nota: Insertar captura de pantalla de los filtros y la tabla.

**5.10.2. ¿Cómo registrar o editar un cobro?**

1. Haga clic en **Nuevo Cobro** o en **Editar** en la fila correspondiente.
2. Complete o revise los campos:

- **Cliente:** seleccione un cliente registrado; al editar, el campo queda bloqueado.
- **Factura ID:** escriba un número mayor que cero.
- **Método de Pago:** seleccione **Efectivo**, **Transferencia**, **Cheque** o **Tarjeta de Crédito**.
- **Monto:** indique un valor mayor que cero; admite decimales.
- **Estado:** al editar, seleccione **Pendiente**, **Completado** o **Cancelado**.

3. Al crear, la fecha se asigna automáticamente y el cliente debe existir en el registro. El sistema no guarda si la factura o el monto son menores o iguales a cero.
4. Haga clic en **Guardar** o en **Cancelar**.

> Nota: Insertar capturas de pantalla de Nuevo Cobro, Editar Cobro y el botón Guardar.

**5.10.3. ¿Cómo consultar un cobro?**

1. Haga clic en el icono de vista.
2. Revise cliente, Factura ID, método, fecha, monto y estado en **Detalle de Cobro**.
3. Cierre con **Cerrar** o la X.

No se encontró una acción de eliminación para los cobros.

**5.11. Pantalla de Servicios (`/servicios`) — uso del Gerente**

> Nota: Insertar captura de pantalla general de Servicios.

Esta pantalla permite consultar y administrar el catálogo de servicios.

**5.11.1. Filtros y tabla**

Filtre por código, nombre, categoría y estado (**Todos**, **Activo** o **Inactivo**). La tabla muestra ID, código de servicio, nombre, categoría, precio base, estado y acciones. Sus encabezados permiten ordenar por estos datos.

> Nota: Insertar captura de pantalla de filtros y tabla.

**5.11.2. ¿Cómo registrar o editar un servicio?**

1. Haga clic en **Nuevo Servicio** o use **Editar** en una fila.
2. Complete o revise los campos:

- **Código (Servicio ID):** es obligatorio.
- **Nombre:** es obligatorio.
- **Categoría:** es opcional.
- **Precio Base:** introduzca un valor mayor que cero; admite decimales.
- **Estado:** durante la edición puede elegir **Activo** o **Inactivo**.

3. Haga clic en **Guardar** o en **Cancelar**. El código, el nombre y un precio positivo son necesarios para guardar.

> Nota: Insertar capturas de pantalla de Nuevo Servicio, Editar Servicio y el botón Guardar.

**5.11.3. ¿Cómo consultar o eliminar un servicio?**

1. Haga clic en el icono de vista para abrir **Detalle de Servicio** y luego seleccione **Cerrar**.
2. Para eliminar, abra la edición del servicio y pulse **Eliminar**.
3. Confirme con **Eliminar** o salga con **Cancelar**. La operación puede rechazarse si el servicio tiene solicitudes asociadas.

> Nota: Insertar capturas de pantalla del detalle y de la confirmación de eliminación.

**5.12. Pantalla de Inventario (`/inventario`) — uso del Gerente**

> Nota: Insertar captura de pantalla general de Inventario.

Esta sección permite consultar y mantener los artículos y sus existencias.

**5.12.1. Filtros y tabla**

Filtre por código, nombre y categoría. La tabla muestra ID, código, nombre, categoría, cantidad, precio unitario y acciones. Las cantidades menores de 20 se resaltan como stock bajo.

> Nota: Insertar captura de pantalla de filtros y tabla de artículos.

**5.12.2. ¿Cómo registrar o editar un artículo?**

1. Haga clic en **Nuevo Artículo** o use **Editar** en el artículo que desea modificar.
2. Complete o modifique:

- **Código** y **Nombre:** son obligatorios.
- **Categoría:** es opcional.
- **Cantidad en Stock:** indique las unidades disponibles; no se aceptan valores negativos.
- **Precio Unitario:** indique un valor igual o superior a cero; admite decimales.

3. Haga clic en **Guardar** o en **Cancelar**. Si falta el código o nombre, o la cantidad o el precio son negativos, el sistema muestra un aviso.

> Nota: Insertar capturas de pantalla de Nuevo Artículo, Editar Artículo y del botón Guardar.

**5.12.3. ¿Cómo consultar un artículo?**

Use el icono de vista. El detalle muestra código, nombre, categoría, stock actual y precio unitario.

No se encontró una acción de eliminación para artículos.

**5.13. Pantalla de Proveedores (`/proveedores`) — uso del Gerente**

> Nota: Insertar captura de pantalla general de Proveedores.

Esta sección permite buscar, registrar, editar y consultar proveedores.

**5.13.1. Filtros y tabla**

Filtre por ID, nombre, RNC y teléfono. La tabla muestra ID, nombre, RNC, correo, teléfono y acciones de vista y edición.

> Nota: Insertar captura de pantalla de filtros y tabla de proveedores.

**5.13.2. ¿Cómo registrar o editar un proveedor?**

1. Haga clic en **Nuevo Proveedor** o en **Editar** para actualizar uno existente.
2. Complete o revise los datos:

- **Nombre** y **RNC:** son obligatorios.
- **Correo**, **teléfono** y **dirección:** son opcionales.

3. Si falta nombre o RNC, el sistema muestra un aviso.
4. Haga clic en **Guardar** o en **Cancelar**.

> Nota: Insertar capturas de pantalla de los formularios y del botón Guardar.

**5.13.3. ¿Cómo consultar un proveedor?**

Use el icono de vista. **Detalle de Proveedor** muestra nombre, RNC, correo, teléfono y dirección. Cierre con **Cerrar** o la X.

No se encontró una acción de eliminación para proveedores.

**5.14. Pantalla de Reportes (`/reportes`) — uso del Gerente**

> Nota: Insertar captura de pantalla general del catálogo de Reportes.

Esta sección reúne los reportes disponibles y las opciones que todavía no están habilitadas.

**5.14.1. Catálogo de reportes**

**Consumo en Servicios** conduce a los reportes de consumo. **Reportes Financieros** y **Rendimiento de Técnicos** aparecen deshabilitados como **Próximamente**.

> Nota: Insertar captura de pantalla del catálogo y las opciones Próximamente.

**5.14.2. ¿Cómo consultar el reporte anual de consumo?**

1. Abra **Consumo en Servicios**.
2. Seleccione **Reporte de Consumo en Servicios del último año**. **Reporte de Consumo Mensual** y **Reporte de Materiales por Técnico** aparecen como **Próximamente**.
3. Complete los filtros:

- **Período:** seleccione **Último Año**.
- **Año:** elija 2025, 2024, 2023, 2022 o 2021.
- **Categoría:** seleccione una categoría para acotar el reporte o conserve todas para ver el total.

4. Haga clic en **Actualizar Reporte**.

El reporte muestra total de consumo, costo promedio por instalación, costo de inventario en riesgo de merma y servicio con mayor consumo. También incluye un gráfico de dona por categoría, una tabla de consumo por tipo de servicio y el gráfico de barras **Tendencia Mensual de Consumo (Costo)**. Las migas de pan permiten volver al catálogo anterior.

> Nota: Insertar capturas de pantalla de los filtros, indicadores, gráfico y tabla generados.

**5.15. Pantalla de Historial de Servicios (`/historial`)**

> Nota: Insertar captura de pantalla general de Historial.

Esta sección permite consultar servicios finalizados.

**5.15.1. Filtros y resultados**

Busque por ID, cliente, tipo de servicio y período (**Todas las fechas**, **Últimos 7 días** o **Últimos 30 días**). La tabla presenta ID, cliente, servicio, fecha de finalización, estado y una acción para consultar el registro.

> Nota: Insertar captura de pantalla de filtros y resultados.

**5.15.2. ¿Cómo consultar un servicio finalizado?**

1. Localice el registro y haga clic en el icono de vista.
2. Revise los datos disponibles en **Detalle de Historial**.
3. Cierre la ventana con **Cerrar** o la X.

Si no existen servicios finalizados, la página muestra un estado vacío.

> Nota: Insertar captura de pantalla del detalle del historial.

**5.16. Pantalla de Gestión de Usuarios (`/usuarios`) — acceso del Gerente**

> Nota: Insertar captura de pantalla de la página Gestión de Usuarios.

La página presenta un listado con usuario, nombre, cargo, rol y acciones visibles **Modificar** y **Eliminar**.

**Importante:** Los botones **Modificar** y **Eliminar** no tienen una acción implementada; no se abren formularios ni ventanas para completar esos flujos. No utilice esos controles como operaciones disponibles.

La ruta comprueba que la sesión corresponda al rol Gerente. Si la sesión no es de Gerente, la aplicación redirige a `/`.

**5.17. Pantallas auxiliares**

Estas páginas no corresponden a los flujos operativos principales. Se incluyen como referencia si forman parte del alcance del manual.

- **`/Modelo/Index`:** plantilla genérica con encabezado «Modelo» y enlace «Crear».
- **`/Error` y `/not-found`:** páginas técnicas de error o de ruta no encontrada.
