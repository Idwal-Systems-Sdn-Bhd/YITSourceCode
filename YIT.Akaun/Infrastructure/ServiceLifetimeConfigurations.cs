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
            services.AddTransient<IAbBukuVotRepository<AbBukuVot>, AbBukuVotRepository>();

            services.AddTransient<IPenyataRepository, PenyataRepository>();
            services.AddTransient<IAkPanjarLejarRepository, AkPanjarLejarRepository>();

            services.AddTransient<_IUnitOfWork, _UnitOfWork>();

            services.AddScoped(ss => SessionCartJKW.GetCart(ss));
            services.AddScoped(ss => SessionCartAkTerima.GetCart(ss));
            services.AddScoped(ss => SessionCartAkPenilaianPerolehan.GetCart(ss));
            services.AddScoped(ss => SessionCartAbWaran.GetCart(ss));
            services.AddScoped(ss => SessionCartAkNotaMinta.GetCart(ss));
            services.AddScoped(ss => SessionCartAkPO.GetCart(ss));
            services.AddScoped(ss => SessionCartAkInden.GetCart(ss));
            services.AddScoped(ss => SessionCartAkPelarasanPO.GetCart(ss));
            services.AddScoped(ss => SessionCartAkPelarasanInden.GetCart(ss));
            services.AddScoped(ss => SessionCartAkBelian.GetCart(ss));
            services.AddScoped(ss => SessionCartAkPV.GetCart(ss));
            services.AddScoped(ss => SessionCartAkJanaanProfil.GetCart(ss));
            services.AddScoped(ss => SessionCartAkEFT.GetCart(ss));
            services.AddScoped(ss => SessionCartAkJurnal.GetCart(ss));
            services.AddScoped(ss => SessionCartJKonfigPerubahanEkuiti.GetCart(ss));
            services.AddScoped(ss => SessionCartJKonfigPenyata.GetCart(ss));
            services.AddScoped(ss => SessionCartDPanjar.GetCart(ss));
            services.AddScoped(ss => SessionCartAkCV.GetCart(ss));

            return services;
        }
    }
}
