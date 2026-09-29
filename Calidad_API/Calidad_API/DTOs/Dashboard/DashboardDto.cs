namespace Calidad_API.DTOs.Dashboard;

public class DashboardDateRange
{
    public DateTime StartDate {get; set;}
    public DateTime EndDate {get; set;}
}

public class DashboardDto
{
    public DefectosRegistradosDto DefectosRegistrados {get; set;}
    public OperacionesRegistradasDto Operaciones {get; set;}
    public DefectoMasEncontradoDto DefectoMasEncontrado {get; set;}
    public DefectosCriticosDto DefectosCriticos {get; set;}
    public ProgramasDto Programas {get; set;}
    public LotesDto Lotes {get; set;}
    public List<NombreValorDto> DefectosPorOperacion { get; set; }
    public List<NombreValorDto> DefectosPorTipoInspeccion { get; set; }
    public List<NombreValorDto> TopDefectos {  get; set; }
    public SerieNombreValorDto defectosPorTiempo { get; set; }
}

public class DefectosRegistradosDto
{
    public int Valor {get; set;}
}

public class OperacionesRegistradasDto
{
    public int Valor {get; set;}
    public List<string> Operaciones {get; set;} = [];
}

public class DefectoMasEncontradoDto
{
    public int Valor {get; set;}
    public string Defecto {get; set;}
    public string Operacion {get; set;}
}

public class DefectosCriticosDto
{
    public int Valor {get; set;}
}

public class ProgramasDto
{
    public int Valor {get; set;}
    public string TopNombre {get; set;}
    public int TopValor {get; set;}
}

public class LotesDto
{
    public int Valor {get; set;}
    public string TopNombre {get; set;}
    public int TopValor {get; set;}
}

public class NombreValorDto
{
    public int Valor {  get; set;}
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