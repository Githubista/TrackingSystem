﻿using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMq;
using StorageService.Consumers;

namespace StorageService.Extensions
{
    internal static class MassTransitConfigurationExtension
    {
        internal static IServiceCollection ConfigureMassTransit(this IServiceCollection services, IConfiguration configuration)
        {
            var rabbitMqSettings = configuration.GetSection(nameof(RabbitMqSettings)).Get<RabbitMqSettings>();

            services.AddMassTransit(x =>
            {
                x.AddConsumer<VisitorTrackedEventConsumer>();
               
                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(rabbitMqSettings.Host, rabbitMqSettings.Port, "/", h =>
                    {
                        h.Username(rabbitMqSettings.Username);
                        h.Password(rabbitMqSettings.Password);
                    });
                });
            });

            services.AddHostedService<ReceiveEndpointsService>();

            return services;
        }
    }
}
