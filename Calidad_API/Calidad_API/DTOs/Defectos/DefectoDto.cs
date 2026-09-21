namespace Calidad_API.DTOs.Defectos
{
    public record DefectoDto(
        long IdDefecto,
        long IdOperacion,
<<<<<<< HEAD
=======
        string OperacionNombre,
>>>>>>> 4772ae54ccaefaf661a47359c90261a8750ce6bb
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
