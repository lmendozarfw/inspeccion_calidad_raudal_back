namespace Calidad_API.Interfaces;

public interface IReparacionService
{
    Task<List<ReparacionDto>> GetByAllAsync(long idDetalle);
    Task<ReparacionDto?> CreateAsync(ReparacionCreateDto dto, long idUsuario);
}
