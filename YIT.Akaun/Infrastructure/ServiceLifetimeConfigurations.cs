﻿using YIT.__Domain.Entities.Administrations;
using YIT.__Domain.Entities.Models._03Akaun;
using YIT._DataAccess.Repositories.Implementations;
using YIT._DataAccess.Repositories.Interfaces;
using YIT._DataAccess.Services.Cart.Session;

namespace YIT.Akaun.Infrastructure
{
    public static class ServiceLifetimeConfigurations
    {
        public static IServiceCollection AddDIContainer(this IServiceCollection services)
        {
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddTransient<UserServices, UserServices>();
            services.AddTransient<_AppLogIRepository<AppLog, int>, _AppLogRepository>();

            services.AddTransient<IAkAkaunRepository<AkAkaun>, AkAkaunRepository>();

            services.AddTransient<_IUnitOfWork, _UnitOfWork>();

            services.AddScoped(ss => SessionCartAkTerima.GetCart(ss));

            return services;
        }
    }
}