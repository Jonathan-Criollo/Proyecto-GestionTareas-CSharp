namespace GestionTareas.Core.Models;

public interface ITarea
{
    Guid Id { get; }
    string Titulo { get; set; }
    string Descripcion { get; set; }
    bool Completada { get; set; }
    DateTime FechaCreacion { get; }
    DateTime? FechaVencimiento { get; set; }
    TipoTarea Tipo { get; }

    void MarcarCompletada();
    string ObtenerDetalle();
}