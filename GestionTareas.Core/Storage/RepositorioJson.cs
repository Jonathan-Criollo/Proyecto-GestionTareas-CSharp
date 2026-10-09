using System.Text.Json;
using System.Text.Json.Serialization;
using GestionTareas.Core.Models;

namespace GestionTareas.Core.Storage;

/// <summary>
/// Persiste las tareas en un archivo JSON usando System.Text.Json.
/// </summary>
public class RepositorioJson : IRepositorioTareas
{
    private static readonly JsonSerializerOptions OpcionesJson = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public string RutaArchivo { get; }

    /// <summary>
    /// Ruta por defecto: %LOCALAPPDATA%\GestionTareas\tareas.json
    /// </summary>
    public static string RutaPorDefecto => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "GestionTareas",
        "tareas.json");

    public RepositorioJson(string? rutaArchivo = null)
    {
        RutaArchivo = string.IsNullOrWhiteSpace(rutaArchivo) ? RutaPorDefecto : rutaArchivo;
    }

    public IEnumerable<ITarea> Cargar()
    {
        if (!File.Exists(RutaArchivo))
        {
            return Enumerable.Empty<ITarea>();
        }

        try
        {
            var json = File.ReadAllText(RutaArchivo);

            if (string.IsNullOrWhiteSpace(json))
            {
                return Enumerable.Empty<ITarea>();
            }

            var dtos = JsonSerializer.Deserialize<List<TareaDto>>(json, OpcionesJson) ?? new List<TareaDto>();
            return dtos.Select(dto => dto.ATarea()).ToList();
        }
        catch (Exception ex) when (ex is JsonException or ArgumentException)
        {
            throw new InvalidDataException($"El archivo de tareas '{RutaArchivo}' está dañado o tiene un formato inválido.", ex);
        }
    }

    public void Guardar(IEnumerable<ITarea> tareas)
    {
        ArgumentNullException.ThrowIfNull(tareas);

        var directorio = Path.GetDirectoryName(Path.GetFullPath(RutaArchivo));
        if (!string.IsNullOrEmpty(directorio))
        {
            Directory.CreateDirectory(directorio);
        }

        var dtos = tareas.Select(TareaDto.DesdeTarea).ToList();
        var json = JsonSerializer.Serialize(dtos, OpcionesJson);

        // Se escribe primero en un archivo temporal para no corromper el original si algo falla.
        var rutaTemporal = RutaArchivo + ".tmp";
        File.WriteAllText(rutaTemporal, json);
        File.Move(rutaTemporal, RutaArchivo, overwrite: true);
    }
}
