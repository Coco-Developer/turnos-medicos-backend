using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace DataAccess.Interceptors
{
    public class EfDbCommandInterceptor : DbCommandInterceptor
    {
        private readonly ILogger<EfDbCommandInterceptor> _logger;

        public EfDbCommandInterceptor(ILogger<EfDbCommandInterceptor> logger)
        {
            _logger = logger;
        }

        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<DbDataReader> result,
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("EFCore SQL executing: {CommandText}", command.CommandText);
            return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
        }

        public override ValueTask<DbDataReader> ReaderExecutedAsync(
            DbCommand command,
            CommandExecutedEventData eventData,
            DbDataReader result,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("EFCore SQL executed in {ElapsedMilliseconds} ms: {CommandText}",
                eventData.Duration.TotalMilliseconds, command.CommandText);

            return base.ReaderExecutedAsync(command, eventData, result, cancellationToken);
        }

        public override ValueTask<int> NonQueryExecutedAsync(
            DbCommand command,
            CommandExecutedEventData eventData,
            int result,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("EFCore NonQuery executed in {ElapsedMilliseconds} ms: {CommandText}",
                eventData.Duration.TotalMilliseconds, command.CommandText);

            return base.NonQueryExecutedAsync(command, eventData, result, cancellationToken);
        }

        public override ValueTask<object?> ScalarExecutedAsync(
            DbCommand command,
            CommandExecutedEventData eventData,
            object? result,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("EFCore Scalar executed in {ElapsedMilliseconds} ms: {CommandText}",
                eventData.Duration.TotalMilliseconds, command.CommandText);

            return base.ScalarExecutedAsync(command, eventData, result, cancellationToken);
        }
    }
}
