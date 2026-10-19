using GestionTareas.Core.Models;

namespace GestionTareas.Core.Storage;

/// <summary>
/// Contrato de almacenamiento de tareas. Permite intercambiar el medio de
/// persistencia (memoria, archivo JSON, etc.) sin modificar al GestorDeTareas.
/// </summary>
public interface IRepositorioTareas
{
    /// <summary>
    /// Obtiene todas las tareas almacenadas.
    /// </summary>
    IEnumerable<ITarea> Cargar();

    /// <summary>
    /// Reemplaza el contenido almacenado por la colección indicada.
    /// </summary>
    void Guardar(IEnumerable<ITarea> tareas);
}
