using Calidad_API.DTOs.Transfers;
using Calidad_API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Calidad_API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class TransfersController : ControllerBase
    {
        private readonly ITransferService _transferService;

        public TransfersController(ITransferService transferService)
        {
            _transferService = transferService;
        }

        [HttpGet("{id:long}")]
        public async Task<ActionResult<TransferDto>> GetById(long id)
        {
            var t = await _transferService.GetByIdAsync(id);
            return t is null ? NotFound(new { mensaje = "Transfer no encontrado." }) : Ok(t);
        }

        [HttpGet("qr/{*qrRaw}")]
        public async Task<ActionResult<TransferDto>> GetByQr(string qrRaw)
        {
            var t = await _transferService.GetByQrAsync(qrRaw);
            return t is null ? NotFound(new { mensaje = "Transfer no encontrado para ese QR." }) : Ok(t);
        }

        [HttpGet("lote/{lote}")]
        public async Task<ActionResult<TransferDto>> GetByLote(string lote)
        {
            var t = await _transferService.GetByLoteAsync(lote);
            return t is null ? NotFound(new { mensaje = "Transfer no encontrado para ese lote." }) : Ok(t);
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<TransferDto>>> Search(
            [FromQuery] string? lote,
            [FromQuery] string? qr,
            [FromQuery] int take = 20)
        {
            var result = await _transferService.SearchAsync(lote, qr, take);
            return Ok(result);
        }

        /// <summary>
        /// Escaneo principal: crea o actualiza el transfer a partir del QR.
        /// Body: qrRaw, lote (obligatorios), programa, lista, punto, idModelo (opcionales).
        /// </summary>
        [HttpPost("scan")]
        public async Task<ActionResult<TransferDto>> Scan([FromBody] TransferCreateDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.QrRaw) || string.IsNullOrWhiteSpace(dto.Lote))
                return BadRequest(new { mensaje = "QrRaw y Lote son obligatorios." });

            var result = await _transferService.CreateOrUpdateFromScanAsync(dto);
            return Ok(result);
        }
    }
}