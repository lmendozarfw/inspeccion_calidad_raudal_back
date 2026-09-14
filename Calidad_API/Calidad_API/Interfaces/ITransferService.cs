using Calidad_API.DTOs.Transfers;

namespace Calidad_API.Interfaces
{
    public interface ITransferService
    {
        Task<TransferDto?> GetByIdAsync(long id);
        Task<TransferDto?> GetByQrAsync(string qrRaw);
        Task<TransferDto?> GetByLoteAsync(string lote);
        Task<TransferDto> CreateOrUpdateFromScanAsync(TransferCreateDto dto);
        Task<IEnumerable<TransferDto>> SearchAsync(string? lote, string? qr, int take = 20);
    }
}
