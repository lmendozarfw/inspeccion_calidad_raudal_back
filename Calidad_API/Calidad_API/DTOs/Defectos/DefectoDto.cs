namespace Calidad_API.DTOs.Defectos
{
    public record DefectoDto(
        long IdDefecto,
        long IdOperacion,
        string Codigo,
        string Nombre,
        short? IdCriticidad,
        string? CriticidadCodigo,
        bool AplicaPieza,
        long? IdPieza,
        string? PiezaCodigo,
        decimal? Ponderacion,
        bool Activo
    );
}
