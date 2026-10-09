using GestionTareas.Core.Factory;
using GestionTareas.Core.Models;
using GestionTareas.Core.Storage;

namespace GestionTareas.Core.Services;

/// <summary>
/// Componente central que gestiona el ciclo de vida de las tareas (CRUD).
/// Recibe el repositorio por constructor, por lo que funciona igual en memoria o con archivo JSON.
/// Cada operación que modifica datos se persiste automáticamente.
/// </summary>
public class GestorDeTareas
{
    private readonly IRepositorioTareas _repositorio;
    private readonly List<ITarea> _tareas;

    /// <summary>
    /// Se dispara después de agregar, editar, eliminar o cambiar el estado de una tarea.
    /// Permite a la UI refrescar la vista.
    /// </summary>
    public event EventHandler? TareasCambiadas;

    public GestorDeTareas(IRepositorioTareas repositorio)
    {
        _repositorio = repositorio ?? throw new ArgumentNullException(nameof(repositorio));
        _tareas = _repositorio.Cargar().ToList();
    }

    public int Total => _tareas.Count;

    public IReadOnlyList<ITarea> ObtenerTodas()
    {
        return _tareas.AsReadOnly();
    }

    public ITarea? ObtenerPorId(Guid id)
    {
        return _tareas.FirstOrDefault(t => t.Id == id);
    }

    /// <summary>
    /// Crea una tarea mediante TareaFactory y la agrega a la colección.
    /// </summary>
    public ITarea Agregar(
        TipoTarea tipo,
        string titulo,
        string descripcion,
        DateTime? fechaVencimiento = null,
        NivelPrioridad prioridad = NivelPrioridad.Media)
    {
        var tarea = TareaFactory.CrearTarea(tipo, titulo, descripcion, fechaVencimiento, prioridad);
        Agregar(tarea);
        return tarea;
    }

    /// <summary>
    /// Agrega una tarea ya creada (por ejemplo, desde FormTareaModal usando la fábrica).
    /// </summary>
    public void Agregar(ITarea tarea)
    {
        ArgumentNullException.ThrowIfNull(tarea);

        if (_tareas.Any(t => t.Id == tarea.Id))
        {
            throw new InvalidOperationException($"Ya existe una tarea con el Id '{tarea.Id}'.");
        }

        _tareas.Add(tarea);
        GuardarCambios();
    }

    /// <summary>
    /// Modifica los datos editables de una tarea existente. El tipo de tarea no cambia.
    /// </summary>
    /// <param name="prioridad">Solo aplica a tareas prioritarias; si es null se conserva la actual.</param>
    public void Editar(
        Guid id,
        string titulo,
        string descripcion,
        DateTime? fechaVencimiento,
        NivelPrioridad? prioridad = null)
    {
        if (string.IsNullOrWhiteSpace(titulo))
        {
            throw new ArgumentException("El título de la tarea no puede estar vacío.", nameof(titulo));
        }

        var tarea = ObtenerExistente(id);

        if (tarea.Tipo == TipoTarea.ConFecha && fechaVencimiento is null)
        {
            throw new ArgumentException("Una tarea con fecha requiere fecha de vencimiento.", nameof(fechaVencimiento));
        }

        tarea.Titulo = titulo.Trim();
        tarea.Descripcion = descripcion?.Trim() ?? string.Empty;

        // Las tareas simples no manejan fecha de vencimiento.
        tarea.FechaVencimiento = tarea.Tipo == TipoTarea.Simple ? null : fechaVencimiento;

        if (tarea is TareaPrioritaria prioritaria && prioridad.HasValue)
        {
            prioritaria.Prioridad = prioridad.Value;
        }

        GuardarCambios();
    }

    public bool Eliminar(Guid id)
    {
        var tarea = ObtenerPorId(id);
        if (tarea is null)
        {
            return false;
        }

        _tareas.Remove(tarea);
        GuardarCambios();
        return true;
    }

    public void MarcarCompletada(Guid id)
    {
        ObtenerExistente(id).MarcarCompletada();
        GuardarCambios();
    }

    public void CambiarEstado(Guid id, bool completada)
    {
        ObtenerExistente(id).Completada = completada;
        GuardarCambios();
    }

    /// <summary>
    /// Vuelve a leer las tareas desde el repositorio, descartando cambios no guardados.
    /// </summary>
    public void Recargar()
    {
        _tareas.Clear();
        _tareas.AddRange(_repositorio.Cargar());
        TareasCambiadas?.Invoke(this, EventArgs.Empty);
    }

    private ITarea ObtenerExistente(Guid id)
    {
        return ObtenerPorId(id)
            ?? throw new KeyNotFoundException($"No existe una tarea con el Id '{id}'.");
    }

    private void GuardarCambios()
    {
        _repositorio.Guardar(_tareas);
        TareasCambiadas?.Invoke(this, EventArgs.Empty);
    }
}
