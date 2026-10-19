namespace GestionTareas.Core.Models;

public class TareaConFecha : TareaBase
{
    public override TipoTarea Tipo => TipoTarea.ConFecha;

    public TareaConFecha(string titulo, string descripcion, DateTime fechaVencimiento)
        : base(titulo, descripcion)
    {
        FechaVencimiento = fechaVencimiento;
    }

    public override string ObtenerDetalle()
    {
        return $"[Con Fecha] {Titulo} - Límite: {FechaVencimiento:dd/MM/yyyy} - Estado: {(Completada ? "Completada" : "Pendiente")}";
    }
}