# Sistema de Gestión de Tareas

Aplicación de escritorio para Windows que te ayuda a organizar tus tareas pendientes: puedes crear
tareas, marcarlas como completadas, editarlas, eliminarlas y filtrarlas (por ejemplo, ver solo las
pendientes o las que vencen esta semana).

Esta guía tiene dos partes:

1. **Guía para usar la aplicación** (paso a paso, no necesitas saber programación).
2. **Información técnica** (para quien quiera revisar o modificar el código).

---

## 1. Guía para usar la aplicación (paso a paso)

No necesitas saber programar para seguir estos pasos. Solo vas a copiar y pegar algunos comandos en
una ventana del sistema.

### Antes de empezar

- Necesitas una computadora con **Windows** (esta aplicación no funciona en Mac ni en Linux).
- Necesitas conexión a internet, pero solo para el primer paso (instalar una herramienta).

### Paso 1: Instalar la herramienta que permite ejecutar la aplicación

Esta aplicación está construida con una tecnología de Microsoft llamada **.NET**. Para poder abrirla,
tu computadora necesita tener instalado el "motor" que la ejecuta, llamado **.NET SDK 10**.

1. Abre este enlace en tu navegador: https://dotnet.microsoft.com/download/dotnet/10.0
2. Busca la sección **SDK** (no "Runtime") y descarga el instalador para Windows (x64 es lo normal).
3. Abre el archivo descargado y sigue el instalador haciendo clic en "Siguiente" / "Instalar" hasta
   que termine. No necesitas cambiar ninguna opción.
4. Una vez instalado, **reinicia tu computadora** (o al menos cierra y vuelve a abrir cualquier
   ventana de terminal que tengas abierta) para que los cambios se apliquen.

> ¿Cómo sé que quedó bien instalado? Sigue el Paso 3 y escribe `dotnet --version`: si te muestra un
> número como `10.0.xxx`, quedó instalado correctamente.

### Paso 2: Descargar el proyecto a tu computadora

Si te compartieron una carpeta con el proyecto (por ejemplo, un ZIP), descomprímela en un lugar fácil
de encontrar, como el Escritorio.

Si en cambio te dieron un enlace de GitHub:

1. Entra al enlace del repositorio en tu navegador.
2. Haz clic en el botón verde **"Code"** y luego en **"Download ZIP"**.
3. Busca el archivo descargado (normalmente en la carpeta "Descargas") y haz clic derecho →
   **"Extraer todo..."**.

Al final deberías tener una carpeta llamada algo como `Proyecto-GestionTareas-CSharp`.

### Paso 3: Abrir una terminal (PowerShell)

Una terminal es una ventana donde se escriben comandos en vez de hacer clic en botones.

1. Presiona la tecla **Windows** y escribe `PowerShell`.
2. Haz clic en la aplicación **"Windows PowerShell"** que aparece.
3. Se abrirá una ventana azul o negra con texto. Ahí vas a escribir los comandos de los siguientes
   pasos.

### Paso 4: Entrar a la carpeta del proyecto

En la ventana de PowerShell, escribe `cd ` (con un espacio al final) y luego arrastra la carpeta del
proyecto (la que descomprimiste en el Paso 2) directamente dentro de la ventana de PowerShell. Esto
completará automáticamente la ruta. Presiona **Enter**.

Por ejemplo, se vería así:

```powershell
cd "C:\Users\TuUsuario\Desktop\Proyecto-GestionTareas-CSharp"
```

### Paso 5: Ejecutar la aplicación

Copia y pega el siguiente comando en PowerShell y presiona **Enter**:

```powershell
dotnet run --project GestionTareas.UI
```

La primera vez puede tardar uno o dos minutos (está preparando la aplicación). Después se abrirá una
ventana titulada **"Gestión de tareas"**: ya puedes usarla.

> No cierres la ventana de PowerShell mientras usas la aplicación; si la cierras, la aplicación
> también se cerrará.

### Cómo usar la aplicación

- **Nueva tarea**: botón "Nueva tarea" (arriba a la derecha de la tabla). Elige el tipo de tarea
  (Simple, Con fecha o Prioritaria), escribe un título y guarda.
- **Editar**: selecciona una tarea en la tabla y haz clic en "Editar" (o doble clic sobre la fila).
- **Completar / Reabrir**: selecciona una tarea y haz clic en "Completar". Si ya está completada, el
  botón cambia a "Reabrir".
- **Eliminar**: selecciona una tarea y haz clic en "Eliminar" (te pedirá confirmación), o presiona la
  tecla **Supr/Delete** con la fila seleccionada.
- **Buscar y filtrar**: usa la barra superior para buscar por texto, o filtrar por estado, tipo,
  prioridad o rango de fechas de vencimiento.

Tus tareas se guardan automáticamente — no hay un botón "Guardar". Puedes cerrar la aplicación y
volver a abrirla más tarde (repitiendo el Paso 5) y tus tareas seguirán ahí.

### Si algo no funciona

| Problema                                                 | Qué hacer                                                                                                                                                       |
| -------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| PowerShell dice que `dotnet` no se reconoce como comando | El SDK no quedó instalado o falta reiniciar. Repite el Paso 1 y reinicia la computadora.                                                                        |
| Aparece un error al ejecutar el Paso 5                   | Verifica que estás dentro de la carpeta correcta (Paso 4) y que esa carpeta contiene el archivo `SistemaGestionTareas.slnx`.                                    |
| Quiero empezar de cero, sin mis tareas anteriores        | Cierra la aplicación y borra el archivo que está en `%LOCALAPPDATA%\GestionTareas\tareas.json` (puedes pegar esa ruta en el explorador de archivos de Windows). |

---

## 2. Información técnica

### Arquitectura

La solución tiene dos proyectos:

| Proyecto             | Responsabilidad                                                                                 |
| -------------------- | ----------------------------------------------------------------------------------------------- |
| `GestionTareas.Core` | Lógica de negocio: modelos, fábrica de tareas, filtrado y persistencia. No depende de WinForms. |
| `GestionTareas.UI`   | Interfaz de escritorio (WinForms) que consume `GestionTareas.Core`.                             |

```
GestionTareas.Core/
├── Models/      ITarea, TareaBase, TareaSimple, TareaConFecha, TareaPrioritaria, TipoTarea, NivelPrioridad
├── Factory/     TareaFactory               (patrón Factory Method)
├── Services/    GestorDeTareas             (CRUD + evento TareasCambiadas)
│                FiltradoDeTareas           (extensiones LINQ encadenables: estado, tipo, prioridad, texto, rango de fechas)
└── Storage/     IRepositorioTareas         (patrón Repository/Strategy)
                 RepositorioEnMemoria
                 RepositorioJson            (persistencia en archivo JSON)
                 TareaDto

GestionTareas.UI/
├── FormPrincipal.cs    Ventana principal: tabla, filtros, acciones (Nueva/Editar/Completar/Eliminar)
├── FormTareaModal.cs   Modal para crear/editar una tarea
└── Program.cs          Punto de entrada; conecta GestorDeTareas con RepositorioJson
```

`GestorDeTareas` recibe el repositorio por inyección de constructor, por lo que no sabe si los datos
se guardan en memoria o en disco. Más detalle de las decisiones de diseño en
[`doc/Punto3-Persistencia-GestorDeTareas.md`](doc/Punto3-Persistencia-GestorDeTareas.md) y de los
criterios visuales de la UI en [`doc/DESIGN.md`](doc/DESIGN.md).

### Requisitos para desarrollo

- Windows + [.NET SDK 10](https://dotnet.microsoft.com/download/dotnet/10.0) (la solución usa el
  formato `.slnx`, que requiere el SDK de .NET 10 o Visual Studio 2026).
- Opcional: Visual Studio 2026 Community con la carga de trabajo "Desarrollo de escritorio de .NET".

```powershell
git clone https://github.com/Jonathan-Criollo/Proyecto-GestionTareas-CSharp.git
cd Proyecto-GestionTareas-CSharp
dotnet build SistemaGestionTareas.slnx
dotnet run --project GestionTareas.UI
```

### Despliegue (generar un ejecutable distribuible)

Para entregar la aplicación sin que el usuario necesite instalar el SDK ni clonar el repositorio,
genera una versión Release con `dotnet publish` desde la raíz del repositorio.

**Opción A — depende del runtime de .NET instalado en la máquina destino** (ejecutable más liviano):

```powershell
dotnet publish GestionTareas.UI -c Release -r win-x64 --self-contained false
```

**Opción B — autocontenido** (no requiere tener el runtime de .NET 10 instalado en la máquina
destino, pero el ejecutable es más pesado):

```powershell
dotnet publish GestionTareas.UI -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

El resultado queda en `GestionTareas.UI/bin/Release/net10.0-windows/win-x64/publish/`. Copia esa
carpeta (o el `.exe` único, en la opción B) a la máquina destino: con la opción B, el usuario final
solo necesita hacer doble clic, sin seguir la guía de instalación de la sección 1.

### Almacenamiento de datos

```
%LOCALAPPDATA%\GestionTareas\tareas.json
```

- Si el archivo no existe, la aplicación inicia con la lista de tareas vacía y crea la carpeta al
  guardar la primera tarea.
- La escritura es atómica (archivo temporal + reemplazo), por lo que un cierre inesperado no debería
  corromper el archivo.
- Si el archivo existe pero está dañado, la aplicación lanza un error claro en lugar de perder los
  datos silenciosamente.

### Estado del proyecto

El código funcional (modelos, fábrica, filtrado, persistencia y la UI completa) vive en la rama
`develop` del repositorio y sus ramas de feature. La rama `master` quedó desactualizada tras un
revert (PR #3) y aún no incorpora esta implementación; se recomienda fusionar `develop` en `master`
como parte del cierre del proyecto.

### Verificación realizada

- `dotnet build SistemaGestionTareas.slnx` → compilación correcta, 0 advertencias, 0 errores.
- La aplicación inicia correctamente y queda a la espera de interacción del usuario.

La matriz de pruebas funcionales completa está en [`doc/MATRIZ_PRUEBAS.md`](doc/MATRIZ_PRUEBAS.md).
