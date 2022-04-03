﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Geta.Optimizely.GoogleProductFeed.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Geta.Optimizely.ProductFeed.Repositories
{
    public class MigrationService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;

        public MigrationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var myDbContext = scope.ServiceProvider.GetRequiredService<FeedApplicationDbContext>();

            await myDbContext.Database.MigrateAsync(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}