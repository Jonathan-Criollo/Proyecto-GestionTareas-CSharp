# DESIGN.md — Sistema de Gestión de Tareas (UI Desktop)

Reglas visuales extraídas de `FormTareaModal` (#5). Cualquier ventana nueva
(principal, filtros, diálogos) debe seguirlas para que todo se vea como un solo producto.

## Producto
Aplicación de escritorio (Windows Forms) para uso personal e individual. El usuario crea, edita,
filtra y completa tareas muchas veces al día: la interfaz debe ser rápida, legible y sin adorno.

## Color (una sola paleta, un solo acento)
| Uso | Valor |
|---|---|
| Texto principal | `#1F242B` |
| Texto secundario / etiquetas | `#5B6470` |
| Acento (botón primario) | `#2F5D50` |
| Error | `#B3261E` |
| Borde de botón secundario | `#C4C9D0` |
| Fondo | `#FFFFFF` |

El acento se usa solo en la acción principal de cada ventana. Sin degradados.

## Tipografía
Segoe UI 9.5 pt en toda la interfaz. Jerarquía por color (principal vs. secundario), no por tamaños distintos.

## Espaciado y estructura
- Margen de ventana: 24 px.
- Contenido en una sola columna de 400 px; etiqueta arriba del campo (14 px antes, 4 px después).
- Sin tarjetas anidadas, sin íconos decorativos, sin títulos de sección repetidos dentro de la ventana
  (el título de la barra de la ventana ya dice qué es).

## Formularios
- Errores como texto rojo visible debajo del campo (no solo tooltip). Se ocultan al corregir.
- Los campos que no aplican se ocultan; no se deshabilitan sin explicación.
- Enter = acción primaria, Esc = cancelar.
- Botón primario a la derecha con acento; secundario con borde gris. Textos específicos
  ("Crear tarea", "Guardar cambios"), nunca "OK/Aceptar".

## Modales
`ShowDialog(this)`, `FixedDialog`, sin minimizar/maximizar, sin icono en la barra de tareas,
centrado sobre la ventana padre. Devuelven `DialogResult.OK` solo si se guardó.

## Textos
Español neutro, directo. Errores que dicen qué hacer: "Escribe un título para la tarea."
Nombres de enum en pantalla: Simple, Con fecha, Prioritaria · Baja, Media, Alta, Crítica.
