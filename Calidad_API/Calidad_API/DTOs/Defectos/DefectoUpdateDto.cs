namespace Calidad_API.DTOs.Defectos
{
    public record DefectoUpdateDto(
        string Nombre,
        short? IdCriticidad,
        bool AplicaPieza,
        long? IdPieza,
        decimal? Ponderacion,
        bool Activo
    );
}
