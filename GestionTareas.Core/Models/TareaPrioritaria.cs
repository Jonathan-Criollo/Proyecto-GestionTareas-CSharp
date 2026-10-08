namespace GestionTareas.Core.Models;

public class TareaPrioritaria : TareaBase
{
    public override TipoTarea Tipo => TipoTarea.Prioritaria;
    public NivelPrioridad Prioridad { get; set; }

    public TareaPrioritaria(string titulo, string descripcion, NivelPrioridad prioridad, DateTime? fechaVencimiento = null)
        : base(titulo, descripcion)
    {
        Prioridad = prioridad;
        FechaVencimiento = fechaVencimiento;
    }

    public override string ObtenerDetalle()
    {
        return $"[Prioritaria - {Prioridad}] {Titulo} - Estado: {(Completada ? "Completada" : "Pendiente")}";
    }
}