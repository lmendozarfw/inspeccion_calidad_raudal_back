namespace Calidad_API.DTOs.Usuario;

public record PiocheroDefectosDto(
    long IdUsuario,
    string NombreCompleto,
    List<DefectoAsignadoDto> Defectos
);
