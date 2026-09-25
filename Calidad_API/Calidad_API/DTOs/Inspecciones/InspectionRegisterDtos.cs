namespace Calidad_API.DTOs.Inspecciones
{
    /// <summary>
    /// Body que manda el front en un solo POST.
    /// Programa y lote obligatorios; modelo y lista opcionales.
    /// </summary>
    public record InspectionRegisterRequest(
        InspectionContextRequest Contexto,
        IReadOnlyList<InspectionItemRequest> Inspecciones
    );

    public record InspectionContextRequest(
        string Programa,          // obligatorio
        string Lote,              // obligatorio
        string? Modelo = null,    // opcional (código combinación o base)
        string? Lista = null,     // opcional
        string? Punto = null      // opcional
    );

    public record InspectionItemRequest(
        long IdOperacion,
        short IdTipoInspeccion,
        string? Dispositivo,
        string? Observaciones,
        IReadOnlyList<InspectionDefectRequest> Defectos
    );

    public record InspectionDefectRequest(
        long IdDefecto,
        string Lado,                 // "derecho" | "izquierdo" | "ambos"
        string? TipoRegistro = null  // "piocha" | "reproceso" → default PIOCHA
    );

    // —— Respuesta ——
    public record InspectionRegisterResponse(
        long IdTransfer,
        string Lote,
        string Programa,
        int? IdModelo,
        string? ModeloCodigo,
        IReadOnlyList<InspectionCreatedDto> Inspecciones
    );

    public record InspectionCreatedDto(
        long IdInspeccion,
        long IdOperacion,
        short IdTipoInspeccion,
        string Estado,
        IReadOnlyList<InspectionDetalleCreatedDto> Detalles
    );

    public record InspectionDetalleCreatedDto(
        long IdDetalle,
        long IdDefecto,
        string TipoRegistro,
        string? Lado,
        decimal Cantidad
    );
}