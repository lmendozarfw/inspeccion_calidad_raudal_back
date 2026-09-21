namespace Calidad_API.DTOs.Defectos
{
    public record DefectoUpdateDto(
        long IdOperacion,
        string Codigo,
        string Nombre,
        short? IdCriticidad,
        bool AplicaPieza,
        long? IdPieza,
        decimal? Ponderacion,
        List<short> IdsTipoInspeccion,
        bool Activo
    );
}
