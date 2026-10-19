# Matriz de pruebas funcionales - Sistema de Gestión de Tareas

Pruebas de caja negra (a nivel de usuario) sobre la aplicación `GestionTareas.UI`, más un anexo de
pruebas técnicas sobre `GestionTareas.Core` ya ejecutadas por el equipo. Complementa lo documentado en
[`doc/Punto3-Persistencia-GestorDeTareas.md`](Punto3-Persistencia-GestorDeTareas.md).

**Cómo usar esta matriz:** ejecuta cada caso siguiendo los pasos, compara el resultado real contra el
esperado y actualiza la columna **Estado** (`Aprobado` / `Fallido` / `Pendiente`). Si un caso falla,
anota el detalle en una columna de notas o en un issue del repositorio.

Leyenda de estado: ✅ Aprobado · ❌ Fallido · ⬜ Pendiente

---

## 1. Creación de tareas (`FormTareaModal`, `TareaFactory`)

| ID     | Caso de prueba                                         | Pasos                                                                               | Resultado esperado                                                                                                                                                                           | Estado |
| ------ | ------------------------------------------------------ | ----------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------ |
| CRE-01 | Crear tarea Simple                                     | "Nueva tarea" → tipo "Simple" → título y descripción → Guardar                      | La tarea aparece en la tabla como tipo "Simple", sin fecha ni prioridad                                                                                                                      | ✅     |
| CRE-02 | Crear tarea Con fecha                                  | "Nueva tarea" → tipo "Con fecha" → título → marcar fecha de vencimiento → Guardar   | La tarea aparece con la fecha elegida en la columna "Vencimiento"                                                                                                                            | ✅     |
| CRE-03 | Crear tarea Prioritaria                                | "Nueva tarea" → tipo "Prioritaria" → título → elegir prioridad (ej. Alta) → Guardar | La tarea aparece con la prioridad elegida en la columna "Prioridad"                                                                                                                          | ✅     |
| CRE-04 | Título vacío no se guarda                              | "Nueva tarea" → dejar título en blanco → Guardar                                    | Se muestra el error debajo del campo ("Escribe un título para la tarea."); el modal no se cierra; no se crea la tarea                                                                        | ✅     |
| CRE-05 | Tarea "Con fecha" sin elegir una fecha distinta        | "Nueva tarea" → tipo "Con fecha" → no tocar el selector de fecha → Guardar          | **(corregido, ver nota)** El tipo "Con fecha" no tiene casilla para "sin fecha": el selector siempre trae una fecha válida (mañana, por defecto) y la tarea se crea con esa fecha, sin error | ✅     |
| CRE-06 | Título en el límite de longitud (100 caracteres)       | Escribir un título de 100 caracteres o más                                          | Se aceptan hasta 100 caracteres exactos; no se puede escribir más (el campo tiene `MaxLength`)                                                                                               | ✅     |
| CRE-07 | Descripción en el límite de longitud (500 caracteres)  | Escribir una descripción de 500 caracteres o más                                    | Se aceptan hasta 500 caracteres exactos; no se puede escribir más (el campo tiene `MaxLength`)                                                                                               | ✅     |
| CRE-08 | Cancelar la creación                                   | "Nueva tarea" → llenar algunos campos → clic en "Cancelar" (o tecla Esc)            | El modal se cierra, no se crea ninguna tarea nueva                                                                                                                                           | ✅     |
| CRE-09 | Cambiar el tipo dentro del modal oculta/muestra campos | En "Nueva tarea", cambiar el combo de tipo entre Simple / Con fecha / Prioritaria   | Los campos de fecha y prioridad aparecen o desaparecen según el tipo elegido, sin dejar valores de un tipo anterior visibles                                                                 | ✅     |

> **Nota sobre CRE-05 (caso corregido tras probarlo):** el caso original asumía una casilla "sin
> fecha" para tareas "Con fecha", igual a la que sí existe para "Prioritaria". Revisando
> `FormTareaModal.AplicarTipo()`, esa casilla (`ShowCheckBox`) **solo se activa para tipo
> "Prioritaria"**; para "Con fecha" el selector nunca se puede dejar vacío y siempre toma una fecha
> válida. Esto coincide con el valor por defecto de `TareaFactory` (`DateTime.Today.AddDays(1)` si no
> se indica fecha), así que no es un error: el caso de prueba estaba mal planteado y se corrigió.
>
> **Observación de usabilidad (CRE-05, CRE-06, CRE-07 — no bloquea la entrega):**
>
> - El usuario no recibe ningún aviso de que se está usando una fecha por defecto (mañana) en una
>   tarea "Con fecha" si no toca el selector. Se recomienda un texto de ayuda como "Si no eliges una
>   fecha, se usará mañana."
> - Los límites de 100/500 caracteres se aplican correctamente, pero no hay un contador visible
>   (ej. "87/100") que le muestre al usuario cuál es el límite antes de alcanzarlo.

## 2. Edición de tareas

| ID     | Caso de prueba                                           | Pasos                                                                                      | Resultado esperado                                                                                                                                                                                                                                                        | Estado |
| ------ | -------------------------------------------------------- | ------------------------------------------------------------------------------------------ | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------ |
| EDI-01 | Editar título y descripción                              | Seleccionar una tarea → "Editar" → cambiar título/descripción → Guardar                    | La tabla muestra los datos actualizados                                                                                                                                                                                                                                   | ✅     |
| EDI-02 | Editar fecha de una tarea "Con fecha"                    | Seleccionar tarea Con fecha → "Editar" → cambiar la fecha → Guardar                        | La columna "Vencimiento" refleja la nueva fecha                                                                                                                                                                                                                           | ✅     |
| EDI-03 | Cambiar prioridad de una tarea Prioritaria               | Seleccionar tarea Prioritaria → "Editar" → cambiar prioridad → Guardar                     | La columna "Prioridad" refleja el nuevo valor                                                                                                                                                                                                                             | ✅     |
| EDI-04 | Intentar dejar sin fecha una tarea "Con fecha" al editar | **(corregido, ver nota)** Editar una tarea Con fecha → buscar una forma de quitar la fecha | No es alcanzable desde la UI: igual que en la creación (CRE-05), el selector de fecha para "Con fecha" no tiene casilla; siempre queda una fecha válida. La validación "fecha requerida" solo existe a nivel de `GestorDeTareas.Editar` y ya está cubierta por **TEC-04** | ✅     |
| EDI-05 | Doble clic abre edición con datos precargados            | Doble clic sobre una fila de la tabla                                                      | Se abre "Editar tarea" con todos los campos ya llenos con los valores actuales                                                                                                                                                                                            | ✅     |
| EDI-06 | Cancelar edición no modifica la tarea                    | Editar una tarea, cambiar algún campo, clic en "Cancelar"                                  | La tarea conserva sus valores originales en la tabla                                                                                                                                                                                                                      | ✅     |
| EDI-07 | El tipo de tarea no se puede cambiar al editar           | Abrir "Editar" sobre una tarea existente                                                   | El combo de tipo aparece deshabilitado (`_cmbTipo.Enabled = !EsEdicion`)                                                                                                                                                                                                  | ✅     |

## 3. Completar y reabrir

| ID     | Caso de prueba                                            | Pasos                                     | Resultado esperado                                                                                                  | Estado |
| ------ | --------------------------------------------------------- | ----------------------------------------- | ------------------------------------------------------------------------------------------------------------------- | ------ |
| EST-01 | Marcar una tarea pendiente como completada                | Seleccionar tarea pendiente → "Completar" | Estado cambia a "Completada", el texto de la fila cambia a gris (color secundario), el botón pasa a decir "Reabrir" | ✅     |
| EST-02 | Reabrir una tarea completada                              | Seleccionar tarea completada → "Reabrir"  | Estado vuelve a "Pendiente", el botón vuelve a decir "Completar"                                                    | ✅     |
| EST-03 | Sin selección, los botones de acción están deshabilitados | Abrir la app sin seleccionar ninguna fila | "Editar", "Eliminar" y "Completar" aparecen deshabilitados                                                          | ⬜     |

## 4. Eliminación

| ID     | Caso de prueba                    | Pasos                                                               | Resultado esperado                                                                                                              | Estado |
| ------ | --------------------------------- | ------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------- | ------ |
| ELI-01 | Eliminar confirmando              | Seleccionar tarea → "Eliminar" → "Sí" en el diálogo de confirmación | La tarea desaparece de la tabla y del contador                                                                                  | ✅     |
| ELI-02 | Cancelar la eliminación           | Seleccionar tarea → "Eliminar" → "No" en el diálogo                 | La tarea permanece en la tabla sin cambios                                                                                      | ✅     |
| ELI-03 | Eliminar con la tecla Supr/Delete | Seleccionar una fila → presionar tecla Delete                       | Se comporta igual que el botón "Eliminar" (pide confirmación), porque `KeyDown` invoca el mismo método `EliminarSeleccionada()` | ✅     |

## 5. Búsqueda y filtros (`FiltradoDeTareas`)

| ID     | Caso de prueba                                | Pasos                                                                                | Resultado esperado                                                                                                                                                                                       | Estado |
| ------ | --------------------------------------------- | ------------------------------------------------------------------------------------ | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------ |
| FIL-01 | Buscar por texto en el título                 | Escribir en "Buscar" una palabra presente solo en el título de una tarea             | Solo esa tarea aparece en la tabla                                                                                                                                                                       | ✅     |
| FIL-02 | Buscar por texto en la descripción            | Escribir en "Buscar" una palabra presente solo en la descripción                     | Esa tarea aparece en los resultados                                                                                                                                                                      | ✅     |
| FIL-03 | Búsqueda sin distinguir mayúsculas/minúsculas | Buscar una palabra en mayúsculas cuando en la tarea está en minúsculas (o viceversa) | La tarea aparece igual en los resultados (`StringComparison.OrdinalIgnoreCase`)                                                                                                                          | ✅     |
| FIL-04 | Filtrar por estado "Pendientes"               | Combo "Estado" → "Pendientes"                                                        | Solo se listan tareas no completadas                                                                                                                                                                     | ✅     |
| FIL-05 | Filtrar por estado "Completadas"              | Combo "Estado" → "Completadas"                                                       | Solo se listan tareas completadas                                                                                                                                                                        | ✅     |
| FIL-06 | Filtrar por tipo                              | Combo "Tipo" → elegir "Con fecha" (por ejemplo)                                      | Solo aparecen tareas de ese tipo                                                                                                                                                                         | ✅     |
| FIL-07 | Filtrar por prioridad                         | Combo "Prioridad" → elegir "Alta"                                                    | Solo aparecen tareas Prioritarias con esa prioridad; tareas Simple/Con fecha no aparecen (`PorPrioridad` descarta explícitamente lo que no es `TareaPrioritaria`)                                        | ✅     |
| FIL-08 | Filtrar por rango de fechas de vencimiento    | Marcar "Vence desde" y "Vence hasta" con un rango que incluya solo algunas tareas    | Solo aparecen tareas cuyo vencimiento cae dentro del rango; tareas sin fecha quedan excluidas                                                                                                            | ✅     |
| FIL-09 | Combinar varios filtros a la vez              | Escribir texto de búsqueda + elegir estado + elegir tipo al mismo tiempo             | La tabla muestra solo tareas que cumplen todas las condiciones simultáneamente (los filtros se encadenan con AND)                                                                                        | ✅     |
| FIL-10 | Quitar los filtros muestra todo de nuevo      | Volver todos los combos a "Todas", borrar el texto de búsqueda, desmarcar fechas     | La tabla vuelve a mostrar todas las tareas                                                                                                                                                               | ✅     |
| FIL-11 | El contador de resultados es correcto         | Aplicar un filtro que deje, por ejemplo, 2 de 5 tareas                               | Dice "2 de 5 tareas" si el filtro deja 2 de un total de 5; si no hay filtro activo y el total es exactamente 1, dice "1 tarea" en singular (el plural depende del total general, no del número filtrado) | ✅     |
| FIL-12 | La selección se conserva al refrescar         | Seleccionar una tarea, luego editarla (sin cambiar los filtros activos)              | Al volver de guardar, la misma tarea sigue seleccionada en la tabla                                                                                                                                      | ✅     |

## 6. Persistencia de datos

| ID     | Caso de prueba                                                    | Pasos                                                                                                                        | Resultado esperado                                                                                       | Estado |
| ------ | ----------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------- | ------ |
| PER-01 | Las tareas se guardan en disco automáticamente                    | Crear una tarea → sin cerrar la app, revisar que exista `%LOCALAPPDATA%\GestionTareas\tareas.json`                           | El archivo existe y contiene la tarea creada                                                             | ✅     |
| PER-02 | Las tareas persisten entre sesiones                               | Crear 2-3 tareas → cerrar la aplicación → volver a abrirla (`dotnet run --project GestionTareas.UI`)                         | Todas las tareas siguen apareciendo igual que antes de cerrar                                            | ✅     |
| PER-03 | El identificador y la fecha de creación no cambian entre sesiones | Anotar el orden/fecha de creación de una tarea → cerrar y reabrir la app → editarla                                          | La tarea sigue siendo "la misma" (no se duplica ni cambia su fecha de creación)                          | ✅     |
| PER-04 | Primer arranque sin archivo de datos                              | Eliminar `tareas.json` (con la app cerrada) → abrir la app                                                                   | La app abre con la tabla vacía, sin errores                                                              | ✅     |
| PER-05 | Archivo de datos corrupto                                         | Con la app cerrada, abrir `tareas.json` en el Bloc de notas y escribir texto inválido (ej. `{rota`) → guardar → abrir la app | La app muestra un mensaje de error claro en vez de cerrarse sin explicación o perder el archivo original | ✅     |
| PER-06 | El estado "Completada" persiste                                   | Marcar una tarea como completada → cerrar y reabrir la app                                                                   | La tarea sigue apareciendo como "Completada"                                                             | ✅     |

> **PER-01 a PER-06** se verificaron de forma automatizada con un script de consola que reproduce
> exactamente la misma secuencia (`new GestorDeTareas(new RepositorioJson(ruta))` → crear/editar
> tareas → crear una _segunda_ instancia sobre el mismo archivo, igual que ocurre al cerrar y volver a
> abrir la ventana real). Se recomienda que alguien del equipo repita PER-02 y PER-05 manualmente
> sobre la ventana real al menos una vez, para confirmar que la UI se comporta igual.

## 7. Casos de interfaz y robustez general

| ID    | Caso de prueba                                  | Pasos                                                                       | Resultado esperado                                                                                                                                 | Estado |
| ----- | ----------------------------------------------- | --------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------- | ------ |
| UI-01 | Crear varias tareas de los tres tipos mezcladas | Crear al menos 2 tareas Simple, 2 Con fecha y 2 Prioritarias                | La tabla y los filtros funcionan correctamente con una mezcla de tipos (todo el código de filtros/columnas opera sobre `ITarea` de forma genérica) | ✅     |
| UI-02 | Volumen moderado de tareas                      | Crear 20-30 tareas (pueden ser de prueba)                                   | La tabla sigue siendo usable, sin errores ni cuelgues visibles                                                                                     | ✅     |
| UI-03 | Cerrar la aplicación desde la "X" de la ventana | Con tareas sin cambios pendientes, cerrar con el botón de cerrar de Windows | La aplicación se cierra sin mensajes de error (no hay lógica personalizada en `FormClosing`; cada cambio ya se guardó al hacerlo)                  | ✅     |
| UI-04 | Redimensionar la ventana                        | Agrandar/achicar la ventana principal arrastrando una esquina               | La tabla y los filtros se ajustan sin solaparse ni cortarse (respeta el tamaño mínimo)                                                             | ⬜     |

---

## Anexo: pruebas técnicas sobre `GestionTareas.Core`

Estas pruebas requieren entorno de desarrollo (no son de caja negra sobre la UI). El equipo ya las
había ejecutado con una aplicación de consola temporal, según
[`doc/Punto3-Persistencia-GestorDeTareas.md`](Punto3-Persistencia-GestorDeTareas.md#8-verificación-realizada);
se re-verificaron aquí con un script equivalente (12/12 aprobadas) y se listan para trazabilidad y por
si se automatizan como pruebas unitarias más adelante.

| ID     | Caso de prueba                                                     | Resultado esperado                                             | Estado |
| ------ | ------------------------------------------------------------------ | -------------------------------------------------------------- | ------ |
| TEC-01 | `TareaFactory.CrearTarea` con título vacío                         | Lanza `ArgumentException`                                      | ✅     |
| TEC-02 | `GestorDeTareas.Agregar` con un `Id` repetido                      | Lanza `InvalidOperationException`                              | ✅     |
| TEC-03 | `GestorDeTareas.Editar` con un `Id` inexistente                    | Lanza `KeyNotFoundException`                                   | ✅     |
| TEC-04 | `GestorDeTareas.Editar` sobre tarea "Con fecha" sin fecha          | Lanza `ArgumentException`                                      | ✅     |
| TEC-05 | `RepositorioJson` con archivo corrupto                             | Lanza `InvalidDataException` con mensaje claro                 | ✅     |
| TEC-06 | `RepositorioEnMemoria` no persiste entre instancias                | Al crear una nueva instancia, la lista está vacía              | ✅     |
| TEC-07 | Evento `TareasCambiadas` se dispara en cada operación de escritura | El evento se invoca tras Agregar/Editar/Eliminar/CambiarEstado | ✅     |

## Resumen de cobertura

| Módulo             | Casos  |
| ------------------ | ------ |
| Creación de tareas | 9      |
| Edición de tareas  | 7      |
| Completar/reabrir  | 3      |
| Eliminación        | 3      |
| Búsqueda y filtros | 12     |
| Persistencia       | 6      |
| UI y robustez      | 4      |
| Técnicas (Core)    | 7      |
| **Total**          | **51** |
