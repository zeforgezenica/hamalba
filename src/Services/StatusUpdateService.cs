// Services/OglasiStatusService.cs
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using hamalba.DataBase;
using Microsoft.EntityFrameworkCore;
using hamalba.Models;

namespace hamalba.Services
{
    public class OglasiStatusService : BackgroundService
    {
        private readonly ILogger<OglasiStatusService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1); // Gleda svakih 1min

        public OglasiStatusService(
            ILogger<OglasiStatusService> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Oglasi Status Service is running");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await UpdateOglasiStatus();
                    _logger.LogInformation("Completed oglasi status update check at: {time}", DateTimeOffset.Now);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while updating oglasi statuses");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }
        }

        private async Task UpdateOglasiStatus()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var currentDateTime = DateTime.Now;

                // 1. Update iz CekaNaObjavu u Aktivan kada datum dodje,
                var oglasToActivate = await dbContext.Oglasi
                    .Where(o => o.Status == OglasStatus.CekaNaObjavu && o.DatumObjave <= currentDateTime)
                    .ToListAsync();

                foreach (var oglas in oglasToActivate)
                {
                    oglas.Status = OglasStatus.Aktivan;
                    _logger.LogInformation("Oglas ID: {OglasId} activated at {Time}", oglas.OglasId, currentDateTime);
                }

                // 2. Kada rok prodje, promjeni status u Zavrsen
                var oglasToComplete = await dbContext.Oglasi
                    .Where(o => o.Status == OglasStatus.Aktivan && o.Rok <= currentDateTime)
                    .ToListAsync();

                foreach (var oglas in oglasToComplete)
                {
                    oglas.Status = OglasStatus.Zavrsen;
                    _logger.LogInformation("Oglas ID: {OglasId} marked as completed at {Time}", oglas.OglasId, currentDateTime);
                }

                // Sprema sve u bazu
                if (oglasToActivate.Any() || oglasToComplete.Any())
                {
                    await dbContext.SaveChangesAsync();
                    _logger.LogInformation("Updated {ActivateCount} oglasi to Aktivan and {CompleteCount} oglasi to Zavrsen",
                        oglasToActivate.Count, oglasToComplete.Count);
                }
            }
        }
    }
}