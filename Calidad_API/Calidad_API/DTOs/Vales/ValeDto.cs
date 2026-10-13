using System.Text.Json.Serialization;

namespace Calidad_API.DTOs.Vales
{
    public record ValeDto(
        long IdVale,
        string Folio,
        long IdInspeccion,
        long IdOperacion,
        string NombreOperacion,
        long IdUsuarioSolicita,
        string UsuarioNombre,
        DateTime FechaGeneracion,
        [property: JsonIgnore] string? RutaPdf,
        string Estado,
        IReadOnlyList<ValeLineaDto> Lineas,
        string? PdfUrl
    );
}
