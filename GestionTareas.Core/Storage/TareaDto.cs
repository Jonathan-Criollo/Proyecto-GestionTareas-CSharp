using GestionTareas.Core.Factory;
using GestionTareas.Core.Models;

namespace GestionTareas.Core.Storage;

/// <summary>
/// Objeto plano usado para serializar tareas. Desacopla el formato del archivo
/// de las clases del modelo, que no tienen constructores ni setters aptos para JSON.
/// </summary>
public class TareaDto
{
    public Guid Id { get; set; }
    public TipoTarea Tipo { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public bool Completada { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaVencimiento { get; set; }
    public NivelPrioridad? Prioridad { get; set; }

    public static TareaDto DesdeTarea(ITarea tarea)
    {
        return new TareaDto
        {
            Id = tarea.Id,
            Tipo = tarea.Tipo,
            Titulo = tarea.Titulo,
            Descripcion = tarea.Descripcion,
            Completada = tarea.Completada,
            FechaCreacion = tarea.FechaCreacion,
            FechaVencimiento = tarea.FechaVencimiento,
            Prioridad = (tarea as TareaPrioritaria)?.Prioridad
        };
    }

    /// <summary>
    /// Reconstruye la tarea reutilizando TareaFactory y conserva su Id y fecha de creación originales.
    /// </summary>
    public ITarea ATarea()
    {
        var tarea = TareaFactory.CrearTarea(
            Tipo,
            Titulo,
            Descripcion,
            FechaVencimiento,
            Prioridad ?? NivelPrioridad.Media);

        tarea.Completada = Completada;

        if (tarea is TareaBase tareaBase)
        {
            tareaBase.RestaurarIdentidad(Id, FechaCreacion);
        }

        return tarea;
    }
}
