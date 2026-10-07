namespace Calidad_API.DTOs.Dashboard;

public class DashboardDateRange
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class DashboardDto
{
    public DefectosRegistradosDto DefectosRegistrados { get; set; }
    public OperacionesRegistradasDto Operaciones { get; set; }
    public DefectoMasEncontradoDto DefectoMasEncontrado { get; set; }
    public DefectosCriticosDto DefectosCriticos { get; set; }
    public ProgramasDto Programas { get; set; }
    public LotesDto Lotes { get; set; }
    public List<NombreValorDto> DefectosPorOperacion { get; set; }
    public List<NombreValorDto> DefectosPorTipoInspeccion { get; set; }
    public List<NombreValorDto> TopDefectos { get; set; }
    public SerieNombreValorDto DefectosPorTiempo { get; set; }
    public List<CriticidadDefecto> CriticidadDefectos { get; set; }
    public List<NombreValorDto> DefectosPorLado { get; set; }
    public List<NombreValorDto> DefectosPorPrograma { get; set; }
    public List<NombreValorDto> DefectosPorLote { get; set; }
    public List<TopDefectosPorOperacionDto> TopDefectosPorOperacion { get; set; } = [];
    public List<DefectosPorDiaHoraDto> DefectosPorDiaHora { get; set; } = [];
}

public class DefectosRegistradosDto
{
    public int Valor { get; set; }
}

public class OperacionesRegistradasDto
{
    public int Valor { get; set; }
    public List<string> Operaciones { get; set; } = [];
}

public class DefectoMasEncontradoDto
{
    public int Valor { get; set; }
    public string Defecto { get; set; }
    public string Operacion { get; set; }
}

public class DefectosCriticosDto
{
    public int Valor { get; set; }
}

public class ProgramasDto
{
    public int Valor { get; set; }
    public string TopNombre { get; set; }
    public int TopValor { get; set; }
}

public class LotesDto
{
    public int Valor { get; set; }
    public string TopNombre { get; set; }
    public int TopValor { get; set; }
}

public class NombreValorDto
{
    public int Valor { get; set; }
    public string Nombre
    {
        get; set;
    }
}

public class SerieNombreValorDto
{
    public string Nombre { get; set; }
    public List<NombreValorDto> Series { get; set; }
}

public class CriticidadDefecto
{
    public string Nombre { get; set; } = string.Empty;
    public int Valor { get; set; }
    public string Percentage { get; set; } = string.Empty;
}


/// <summary>Top N defectos dentro de una operación (área).</summary>
public class TopDefectosPorOperacionDto
{
    public long IdOperacion { get; set; }
    public string Operacion { get; set; } = string.Empty;
    public List<NombreValorDto> Defectos { get; set; } = [];
}

/// <summary>Defectos agrupados por día; cada día trae la serie hora a hora (0–23).</summary>
public class DefectosPorDiaHoraDto
{
    public string Fecha { get; set; } = string.Empty; // yyyy-MM-dd
    public List<NombreValorDto> Horas { get; set; } = []; // Nombre = "00".."23"
}

public class DashboardDetalleFiltro
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime? Fecha { get; set; }      // día concreto
    public int? Hora { get; set; }            // 0–23
    public long? IdOperacion { get; set; }
    public long? IdDefecto { get; set; }
    public string? Programa { get; set; }
    public string? Lote { get; set; }
    public string? Lado { get; set; }
    public short? IdCriticidad { get; set; }
}

public record DashboardDetalleItemDto(
    long IdDetalle,
    long IdInspeccion,
    DateTime FechaRegistro,
    string? Programa,
    string Lote,
    string? Lista,
    string? Punto,
    string Operacion,
    string TipoInspeccion,
    long IdDefecto,
    string DefectoCodigo,
    string DefectoNombre,
    string? Criticidad,
    string TipoRegistro,
    string? Lado,
    decimal Cantidad,
    string Usuario
);