using HomeServicePlatform.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Common.Behaviors
{
    public class ConcurrencyRetryBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private const int MaxAttempts = 3;

        private const int BaseDelayMilliseconds = 25;

        private readonly IApplicationDbContext _context;

        public ConcurrencyRetryBehavior(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            for (var attempt = 1; ; attempt++)
            {
                try
                {
                    return await next();
                }
                catch (DbUpdateConcurrencyException) when (attempt < MaxAttempts)
                {
                    _context.ResetTrackedChanges();

                    var delay = BaseDelayMilliseconds * attempt + Random.Shared.Next(0, BaseDelayMilliseconds);
                    await Task.Delay(delay, cancellationToken);
                }
            }
        }
    }
}
