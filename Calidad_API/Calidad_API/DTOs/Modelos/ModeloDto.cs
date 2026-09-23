namespace Calidad_API.DTOs.Modelos
{
    public record ModeloDto(
        int IdModelo,
        string CodigoModeloBase,
        string CodigoCombinacion,
        string Descripcion,
        bool Estatus
    );
    public record ModeloCreateDto(
        string CodigoModeloBase,
        string CodigoCombinacion,
        string Descripcion
    );

    public record ModeloUpdateDto(
        string CodigoModeloBase,
        string CodigoCombinacion,
        string Descripcion,
        bool Estatus
    );
}