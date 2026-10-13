using Calidad_API.Data;
using Calidad_API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Calidad_API.Services
{
    public class CorteValeBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<CorteValeBackgroundService> _logger;

        // Evita correr el mismo corte dos veces el mismo día/hora
        private DateOnly? _ultimoCorte1;
        private DateOnly? _ultimoCorte2;

        public CorteValeBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<CorteValeBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("CorteValeBackgroundService iniciado.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await EvaluarYEjecutarAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error en el ciclo del job de corte de vales.");
                }

                // Revisa cada minuto
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

        private async Task EvaluarYEjecutarAsync(CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var corteService = scope.ServiceProvider.GetRequiredService<ICorteValeService>();

            var cfg = await db.ConfigCortesVale.AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == 1, ct);

            if (cfg is null || !cfg.Activo)
                return;

            TimeZoneInfo tz;
            try
            {
                tz = TimeZoneInfo.FindSystemTimeZoneById(
                    OperatingSystem.IsWindows()
                        ? "Central Standard Time"
                        : "America/Mexico_City");
            }
            catch
            {
                tz = TimeZoneInfo.Local;
            }

            var ahoraLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
            var hoy = DateOnly.FromDateTime(ahoraLocal);

            _ultimoCorte1 = await IntentarCorteAsync(
                corteService, cfg.HoraCorte1, ahoraLocal, hoy, _ultimoCorte1);

            _ultimoCorte2 = await IntentarCorteAsync(
                corteService, cfg.HoraCorte2, ahoraLocal, hoy, _ultimoCorte2);
        }

        private async Task<DateOnly?> IntentarCorteAsync(
            ICorteValeService corteService,
            TimeSpan horaConfig,
            DateTime ahoraLocal,
            DateOnly hoy,
            DateOnly? ultimoDia)
        {
            if (ahoraLocal.Hour == horaConfig.Hours
                && ahoraLocal.Minute == horaConfig.Minutes
                && ultimoDia != hoy)
            {
                _logger.LogInformation(
                    "Ejecutando corte automático a las {Hora} (local {Local})",
                    horaConfig, ahoraLocal);

                var result = await corteService.EjecutarCorteAsync();
                _logger.LogInformation(
                    "Corte automático terminado. Cortes creados: {N}",
                    result.CortesCreados);

                return hoy;
            }

            return ultimoDia;
        }
    }
}