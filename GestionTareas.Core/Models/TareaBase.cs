namespace GestionTareas.Core.Models;

public abstract class TareaBase : ITarea
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public string Titulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public bool Completada { get; set; } = false;
    public DateTime FechaCreacion { get; protected set; } = DateTime.Now;
    public virtual DateTime? FechaVencimiento { get; set; }
    public abstract TipoTarea Tipo { get; }

    protected TareaBase(string titulo, string descripcion)
    {
        Titulo = titulo;
        Descripcion = descripcion;
    }

    public virtual void MarcarCompletada()
    {
        Completada = true;
    }

    public abstract string ObtenerDetalle();

    /// <summary>
    /// Restaura la identidad original de una tarea leída desde almacenamiento.
    /// Es interno para que solo la capa de persistencia pueda usarlo.
    /// </summary>
    internal void RestaurarIdentidad(Guid id, DateTime fechaCreacion)
    {
        Id = id;
        FechaCreacion = fechaCreacion;
    }
}