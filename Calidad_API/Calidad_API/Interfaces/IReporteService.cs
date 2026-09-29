namespace Calidad_API.Interfaces
{
    public interface IReporteService
    {
        Task<byte[]> DefectosDetalleExcelAsync(
            DateTime desde, DateTime hasta,
            long? idOperacion, string? programa, string? lote, string? tipoRegistro);

        Task<byte[]> DefectosResumenExcelAsync(
            DateTime desde, DateTime hasta,
            long? idOperacion, string? programa);

        Task<byte[]> PorLoteExcelAsync(
            DateTime desde, DateTime hasta,
            string? programa, string? lote);

        Task<byte[]> ValesExcelAsync(
            DateTime desde, DateTime hasta,
            string? estado, long? idOperacion);

        Task<byte[]> IndicadoresExcelAsync(
            DateTime desde, DateTime hasta,
            long? idOperacion, string? programa);

        Task<byte[]> InspeccionesExcelAsync(
            DateTime desde, DateTime hasta,
            long? idOperacion, string? lote);
    }
}