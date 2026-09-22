namespace Calidad_API.DTOs.Defectos;

public class DefectoFiltroDto
{
    public List<long>? OperationIds { get; set; }
    public List<short>? InspectionTypeIds { get; set; }
    public bool? RequirePiece { get; set; }
    public List<short>? CriticalityIds { get; set; }
}