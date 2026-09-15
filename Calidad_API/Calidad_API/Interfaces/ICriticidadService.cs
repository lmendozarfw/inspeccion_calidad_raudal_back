using Calidad_API.DTOs.Criticidad;

namespace Calidad_API.Interfaces;

public interface ICriticidadService
{
    Task<IEnumerable<CriticidadDto>> GetAllAsync();
}