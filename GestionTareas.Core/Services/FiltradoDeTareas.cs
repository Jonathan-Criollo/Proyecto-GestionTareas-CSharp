using System;
using System.Collections.Generic;
using System.Linq;
using GestionTareas.Core.Models;

namespace GestionTareas.Core.Services
{
    /// <summary>
    /// Provee métodos de extensión para el filtrado de colecciones de ITarea.
    /// Diseñado bajo un enfoque de Fluent API para permitir la composición de filtros mediante LINQ.
    /// </summary>
    public static class FiltradoDeTareas
    {
        /// <summary>
        /// Filtra las tareas según su estado (Completadas o Pendientes).
        /// </summary>
        public static IEnumerable<ITarea> PorEstado(this IEnumerable<ITarea> tareas, bool completada)
        {
            return tareas.Where(t => t.Completada == completada);
        }

        /// <summary>
        /// Filtra las tareas cuyo vencimiento caiga dentro de un rango de fechas.
        /// Excluye tareas sin fecha de vencimiento.
        /// </summary>
        public static IEnumerable<ITarea> PorRangoDeFechas(this IEnumerable<ITarea> tareas, DateTime fechaInicio, DateTime fechaFin)
        {
            // Se utiliza .Date para evitar inconsistencias por la parte horaria (TimeOfDay).
            return tareas.Where(t => t.FechaVencimiento.HasValue &&
                                     t.FechaVencimiento.Value.Date >= fechaInicio.Date &&
                                     t.FechaVencimiento.Value.Date <= fechaFin.Date);
        }

        /// <summary>
        /// Filtra la colección estrictamente por el TipoTarea.
        /// </summary>
        public static IEnumerable<ITarea> PorTipo(this IEnumerable<ITarea> tareas, TipoTarea tipo)
        {
            return tareas.Where(t => t.Tipo == tipo);
        }

        /// <summary>
        /// Realiza una búsqueda insensible a mayúsculas/minúsculas en el Título y la Descripción.
        /// </summary>
        public static IEnumerable<ITarea> ContieneTexto(this IEnumerable<ITarea> tareas, string textoBusqueda)
        {
            if (string.IsNullOrWhiteSpace(textoBusqueda))
                return tareas;

            // StringComparison.OrdinalIgnoreCase es la opción más eficiente y segura para búsquedas internas.
            return tareas.Where(t =>
                (t.Titulo != null && t.Titulo.Contains(textoBusqueda, StringComparison.OrdinalIgnoreCase)) ||
                (t.Descripcion != null && t.Descripcion.Contains(textoBusqueda, StringComparison.OrdinalIgnoreCase))
            );
        }

        /// <summary>
        /// Filtro especializado para tareas prioritarias aprovechando Pattern Matching.
        /// Ignora silenciosamente las tareas que no sean del tipo TareaPrioritaria.
        /// </summary>
        public static IEnumerable<ITarea> PorPrioridad(this IEnumerable<ITarea> tareas, NivelPrioridad prioridad)
        {
            // El pattern matching (t is TareaPrioritaria tp) realiza el cast seguro de manera eficiente en C#.
            return tareas.Where(t => t is TareaPrioritaria tp && tp.Prioridad == prioridad);
        }
    }
}