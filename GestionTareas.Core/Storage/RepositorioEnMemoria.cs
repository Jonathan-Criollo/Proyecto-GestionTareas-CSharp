using GestionTareas.Core.Models;

namespace GestionTareas.Core.Storage;

/// <summary>
/// Almacena las tareas únicamente en memoria. Los datos se pierden al cerrar la aplicación.
/// Útil para pruebas o para usar el sistema sin archivo.
/// </summary>
public class RepositorioEnMemoria : IRepositorioTareas
{
    private readonly List<ITarea> _tareas = new();

    public IEnumerable<ITarea> Cargar()
    {
        return _tareas.ToList();
    }

    public void Guardar(IEnumerable<ITarea> tareas)
    {
        ArgumentNullException.ThrowIfNull(tareas);

        _tareas.Clear();
        _tareas.AddRange(tareas);
    }
}
