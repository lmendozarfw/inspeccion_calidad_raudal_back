using Calidad_API.Data;
using Calidad_API.DTOs.Transfers;
using Calidad_API.Interfaces;
using Calidad_API.Models;
using Microsoft.EntityFrameworkCore;

namespace Calidad_API.Services
{
    public class TransferService : ITransferService
    {
        private readonly ApplicationDbContext _context;

        public TransferService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<TransferDto?> GetByIdAsync(long id)
        {
            var t = await _context.Transfers
                .AsNoTracking()
                .Include(x => x.Modelo)
                .FirstOrDefaultAsync(x => x.IdTransfer == id);

            return t is null ? null : Map(t);
        }

        public async Task<TransferDto?> GetByQrAsync(string qrRaw)
        {
            var t = await _context.Transfers
                .AsNoTracking()
                .Include(x => x.Modelo)
                .FirstOrDefaultAsync(x => x.QrRaw == qrRaw);

            return t is null ? null : Map(t);
        }

        public async Task<TransferDto?> GetByLoteAsync(string lote)
        {
            var t = await _context.Transfers
                .AsNoTracking()
                .Include(x => x.Modelo)
                .FirstOrDefaultAsync(x => x.Lote == lote);

            return t is null ? null : Map(t);
        }

        public async Task<TransferDto> CreateOrUpdateFromScanAsync(TransferCreateDto dto)
        {
            var existing = await _context.Transfers
                .Include(x => x.Modelo)
                .FirstOrDefaultAsync(x => x.QrRaw == dto.QrRaw || x.Lote == dto.Lote);

            if (existing is not null)
            {
                existing.FechaUltimoScan = DateTime.UtcNow;
                if (!string.IsNullOrWhiteSpace(dto.Programa)) existing.Programa = dto.Programa;
                if (!string.IsNullOrWhiteSpace(dto.Lista)) existing.Lista = dto.Lista;
                if (!string.IsNullOrWhiteSpace(dto.Punto)) existing.Punto = dto.Punto;
                if (dto.IdModelo.HasValue) existing.IdModelo = dto.IdModelo;

                await _context.SaveChangesAsync();
                await _context.Entry(existing).Reference(x => x.Modelo).LoadAsync();
                return Map(existing);
            }

            var entity = new Transfer
            {
                QrRaw = dto.QrRaw.Trim(),
                Programa = dto.Programa?.Trim(),
                Lista = dto.Lista?.Trim(),
                Lote = dto.Lote.Trim(),
                Punto = dto.Punto?.Trim(),
                IdModelo = dto.IdModelo,
                FechaPrimerScan = DateTime.UtcNow,
                FechaUltimoScan = DateTime.UtcNow
            };

            _context.Transfers.Add(entity);
            await _context.SaveChangesAsync();

            if (entity.IdModelo.HasValue)
                await _context.Entry(entity).Reference(x => x.Modelo).LoadAsync();

            return Map(entity);
        }

        public async Task<IEnumerable<TransferDto>> SearchAsync(string? lote, string? qr, int take = 20)
        {
            var query = _context.Transfers.AsNoTracking().Include(x => x.Modelo).AsQueryable();

            if (!string.IsNullOrWhiteSpace(lote))
                query = query.Where(x => x.Lote.Contains(lote));
            if (!string.IsNullOrWhiteSpace(qr))
                query = query.Where(x => x.QrRaw.Contains(qr));

            var list = await query
                .OrderByDescending(x => x.FechaUltimoScan)
                .Take(take)
                .ToListAsync();

            return list.Select(Map);
        }

        private static TransferDto Map(Transfer t) => new(
            t.IdTransfer,
            t.QrRaw,
            t.Programa,
            t.Lista,
            t.Lote,
            t.Punto,
            t.IdModelo,
            t.Modelo?.CodigoMB,
            t.Modelo?.CodigoCombinacion,
            t.Modelo?.Descripcion,
            t.FechaPrimerScan,
            t.FechaUltimoScan
        );
    }
}
