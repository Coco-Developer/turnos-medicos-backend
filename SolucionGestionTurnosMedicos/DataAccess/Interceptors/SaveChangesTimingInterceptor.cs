using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace DataAccess.Interceptors
{
    public class SaveChangesTimingInterceptor : SaveChangesInterceptor
    {
        private readonly ILogger<SaveChangesTimingInterceptor> _logger;

        public SaveChangesTimingInterceptor(ILogger<SaveChangesTimingInterceptor> logger)
        {
            _logger = logger;
        }

        public override ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
        {
            // Log basic info: number of entities saved
            _logger.LogInformation("SaveChangesAsync completed. Entities saved: {Entities}",
                eventData.EntitiesSavedCount);

            return base.SavedChangesAsync(eventData, result, cancellationToken);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("SaveChangesAsync starting...");
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }
    }
}
