namespace Calidad_API.DTOs.Criticidad;

public record CriticidadDto(
    short IdCriticidad,
    string Nombre,
    string Codigo,
    short Nivel);