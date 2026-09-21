namespace Calidad_API.DTOs.Defectos
{
    public record DefectoCreateDto(
        List<long> IdsOperacion,
        string Codigo,
        string Nombre,
        short? IdCriticidad,
        bool AplicaPieza,
        long? IdPieza,
        decimal? Ponderacion,
        List<short> IdsTipoInspeccion
    );
}
