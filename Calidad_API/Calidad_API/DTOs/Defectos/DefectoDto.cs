namespace Calidad_API.DTOs.Defectos
{
    public record DefectoDto(
        long IdDefecto,
        long IdOperacion,
        string OperacionNombre,
        string Codigo,
        string Nombre,
        short? IdCriticidad,
        string? CriticidadCodigo,
        string? CriticidadNombre,
        bool AplicaPieza,
        long? IdPieza,
        string? PiezaCodigo,
        string? PiezaNombre,
        List<short>? IdsTipoInspeccion,
        List<string>? TiposInspeccionNombre,
        decimal? Ponderacion,
        bool Activo
    );
}
