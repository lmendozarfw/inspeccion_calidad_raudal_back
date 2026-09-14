namespace Calidad_API.DTOs.Defectos
{
    public record DefectoCreateDto(
        long IdArea,
        string Codigo,
        string Nombre,
        short? IdCriticidad,
        bool AplicaPieza,
        long? IdPieza,
        decimal? Ponderacion
    );
}
