using GestionTareas.Core.Models;

namespace GestionTareas.Core.Factory;

public static class TareaFactory
{
    /// <summary>
    /// Crea instancias de ITarea desacoplando la creación concreta del consumidor.
    /// </summary>
    public static ITarea CrearTarea(
        TipoTarea tipo,
        string titulo,
        string descripcion,
        DateTime? fechaVencimiento = null,
        NivelPrioridad prioridad = NivelPrioridad.Media)
    {
        if (string.IsNullOrWhiteSpace(titulo))
        {
            throw new ArgumentException("El título de la tarea no puede estar vacío.", nameof(titulo));
        }

        return tipo switch
        {
            TipoTarea.Simple => new TareaSimple(titulo, descripcion),

            TipoTarea.ConFecha => new TareaConFecha(
                titulo,
                descripcion,
                fechaVencimiento ?? DateTime.Today.AddDays(1)
            ),

            TipoTarea.Prioritaria => new TareaPrioritaria(
                titulo,
                descripcion,
                prioridad,
                fechaVencimiento
            ),

            _ => throw new ArgumentOutOfRangeException(nameof(tipo), $"El tipo de tarea '{tipo}' no está soportado.")
        };
    }
}