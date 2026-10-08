namespace GestionTareas.Core.Models;

public class TareaSimple : TareaBase
{
    public override TipoTarea Tipo => TipoTarea.Simple;

    public TareaSimple(string titulo, string descripcion)
        : base(titulo, descripcion)
    {
        FechaVencimiento = null;
    }

    public override string ObtenerDetalle()
    {
        return $"[Simple] {Titulo} - Estado: {(Completada ? "Completada" : "Pendiente")}";
    }
}