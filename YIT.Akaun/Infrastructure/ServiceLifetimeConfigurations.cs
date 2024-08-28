using YIT.__Domain.Entities._Statics;
using YIT.__Domain.Entities.Administrations;
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
            services.AddTransient<IAkAnggarLejarRepository<AkAnggarLejar>, AkAnggarLejarRepository>();

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
            services.AddScoped(ss => SessionCartAkAnggar.GetCart(ss));
            services.AddScoped(ss => SessionCartAkNotaDebitKreditDiterima.GetCart(ss));
            services.AddScoped(ss => SessionCartAkInvois.GetCart(ss));

            return services;
        }
        
        public static IServiceCollection AddSystemAuthorization(this IServiceCollection services)
        {
            services.AddAuthorization(opt =>
            {
                // Sistem (SI)
                //

                // Jadual (JD)
                opt.AddPolicy(Modules.kodJAgama, policy => policy.RequireClaim(Modules.kodJAgama));
                opt.AddPolicy(Modules.kodJAgama + "C", policy => policy.RequireClaim(Modules.kodJAgama + "C"));
                opt.AddPolicy(Modules.kodJAgama + "E", policy => policy.RequireClaim(Modules.kodJAgama + "E"));
                opt.AddPolicy(Modules.kodJAgama + "D", policy => policy.RequireClaim(Modules.kodJAgama + "D"));
                opt.AddPolicy(Modules.kodJAgama + "R", policy => policy.RequireClaim(Modules.kodJAgama + "R"));

                opt.AddPolicy(Modules.kodJBahagian, policy => policy.RequireClaim(Modules.kodJBahagian));
                opt.AddPolicy(Modules.kodJBahagian + "C", policy => policy.RequireClaim(Modules.kodJBahagian + "C"));
                opt.AddPolicy(Modules.kodJBahagian + "E", policy => policy.RequireClaim(Modules.kodJBahagian + "E"));
                opt.AddPolicy(Modules.kodJBahagian + "D", policy => policy.RequireClaim(Modules.kodJBahagian + "D"));
                opt.AddPolicy(Modules.kodJBahagian + "R", policy => policy.RequireClaim(Modules.kodJBahagian + "R"));

                opt.AddPolicy(Modules.kodJBangsa, policy => policy.RequireClaim(Modules.kodJBangsa));
                opt.AddPolicy(Modules.kodJBangsa + "C", policy => policy.RequireClaim(Modules.kodJBangsa + "C"));
                opt.AddPolicy(Modules.kodJBangsa + "E", policy => policy.RequireClaim(Modules.kodJBangsa + "E"));
                opt.AddPolicy(Modules.kodJBangsa + "D", policy => policy.RequireClaim(Modules.kodJBangsa + "D"));
                opt.AddPolicy(Modules.kodJBangsa + "R", policy => policy.RequireClaim(Modules.kodJBangsa + "R"));

                opt.AddPolicy(Modules.kodJBank, policy => policy.RequireClaim(Modules.kodJBank));
                opt.AddPolicy(Modules.kodJBank + "C", policy => policy.RequireClaim(Modules.kodJBank + "C"));
                opt.AddPolicy(Modules.kodJBank + "E", policy => policy.RequireClaim(Modules.kodJBank + "E"));
                opt.AddPolicy(Modules.kodJBank + "D", policy => policy.RequireClaim(Modules.kodJBank + "D"));
                opt.AddPolicy(Modules.kodJBank + "R", policy => policy.RequireClaim(Modules.kodJBank + "R"));

                opt.AddPolicy(Modules.kodJCaraBayar, policy => policy.RequireClaim(Modules.kodJCaraBayar));
                opt.AddPolicy(Modules.kodJCaraBayar + "C", policy => policy.RequireClaim(Modules.kodJCaraBayar + "C"));
                opt.AddPolicy(Modules.kodJCaraBayar + "E", policy => policy.RequireClaim(Modules.kodJCaraBayar + "E"));
                opt.AddPolicy(Modules.kodJCaraBayar + "D", policy => policy.RequireClaim(Modules.kodJCaraBayar + "D"));
                opt.AddPolicy(Modules.kodJCaraBayar + "R", policy => policy.RequireClaim(Modules.kodJCaraBayar + "R"));

                opt.AddPolicy(Modules.kodJCawangan, policy => policy.RequireClaim(Modules.kodJCawangan));
                opt.AddPolicy(Modules.kodJCawangan + "C", policy => policy.RequireClaim(Modules.kodJCawangan + "C"));
                opt.AddPolicy(Modules.kodJCawangan + "E", policy => policy.RequireClaim(Modules.kodJCawangan + "E"));
                opt.AddPolicy(Modules.kodJCawangan + "D", policy => policy.RequireClaim(Modules.kodJCawangan + "D"));
                opt.AddPolicy(Modules.kodJCawangan + "R", policy => policy.RequireClaim(Modules.kodJCawangan + "R"));

                opt.AddPolicy(Modules.kodJKonfigPenyata, policy => policy.RequireClaim(Modules.kodJKonfigPenyata));
                opt.AddPolicy(Modules.kodJKonfigPenyata + "C", policy => policy.RequireClaim(Modules.kodJKonfigPenyata + "C"));
                opt.AddPolicy(Modules.kodJKonfigPenyata + "E", policy => policy.RequireClaim(Modules.kodJKonfigPenyata + "E"));
                opt.AddPolicy(Modules.kodJKonfigPenyata + "D", policy => policy.RequireClaim(Modules.kodJKonfigPenyata + "D"));
                opt.AddPolicy(Modules.kodJKonfigPenyata + "R", policy => policy.RequireClaim(Modules.kodJKonfigPenyata + "R"));

                opt.AddPolicy(Modules.kodJKonfigPerubahanEkuiti, policy => policy.RequireClaim(Modules.kodJKonfigPerubahanEkuiti));
                opt.AddPolicy(Modules.kodJKonfigPerubahanEkuiti + "C", policy => policy.RequireClaim(Modules.kodJKonfigPerubahanEkuiti + "C"));
                opt.AddPolicy(Modules.kodJKonfigPerubahanEkuiti + "E", policy => policy.RequireClaim(Modules.kodJKonfigPerubahanEkuiti + "E"));
                opt.AddPolicy(Modules.kodJKonfigPerubahanEkuiti + "D", policy => policy.RequireClaim(Modules.kodJKonfigPerubahanEkuiti + "D"));
                opt.AddPolicy(Modules.kodJKonfigPerubahanEkuiti + "R", policy => policy.RequireClaim(Modules.kodJKonfigPerubahanEkuiti + "R"));

                opt.AddPolicy(Modules.kodJKW, policy => policy.RequireClaim(Modules.kodJKW));
                opt.AddPolicy(Modules.kodJKW + "C", policy => policy.RequireClaim(Modules.kodJKW + "C"));
                opt.AddPolicy(Modules.kodJKW + "E", policy => policy.RequireClaim(Modules.kodJKW + "E"));
                opt.AddPolicy(Modules.kodJKW + "D", policy => policy.RequireClaim(Modules.kodJKW + "D"));
                opt.AddPolicy(Modules.kodJKW + "R", policy => policy.RequireClaim(Modules.kodJKW + "R"));

                opt.AddPolicy(Modules.kodJNegeri, policy => policy.RequireClaim(Modules.kodJNegeri));
                opt.AddPolicy(Modules.kodJNegeri + "C", policy => policy.RequireClaim(Modules.kodJNegeri + "C"));
                opt.AddPolicy(Modules.kodJNegeri + "E", policy => policy.RequireClaim(Modules.kodJNegeri + "E"));
                opt.AddPolicy(Modules.kodJNegeri + "D", policy => policy.RequireClaim(Modules.kodJNegeri + "D"));
                opt.AddPolicy(Modules.kodJNegeri + "R", policy => policy.RequireClaim(Modules.kodJNegeri + "R"));

                opt.AddPolicy(Modules.kodJPTJ, policy => policy.RequireClaim(Modules.kodJPTJ));
                opt.AddPolicy(Modules.kodJPTJ + "C", policy => policy.RequireClaim(Modules.kodJPTJ + "C"));
                opt.AddPolicy(Modules.kodJPTJ + "E", policy => policy.RequireClaim(Modules.kodJPTJ + "E"));
                opt.AddPolicy(Modules.kodJPTJ + "D", policy => policy.RequireClaim(Modules.kodJPTJ + "D"));
                opt.AddPolicy(Modules.kodJPTJ + "R", policy => policy.RequireClaim(Modules.kodJPTJ + "R"));
                //

                // Daftar (DF)
                opt.AddPolicy(Modules.kodDDaftarAwam, policy => policy.RequireClaim(Modules.kodDDaftarAwam));
                opt.AddPolicy(Modules.kodDDaftarAwam + "C", policy => policy.RequireClaim(Modules.kodDDaftarAwam + "C"));
                opt.AddPolicy(Modules.kodDDaftarAwam + "E", policy => policy.RequireClaim(Modules.kodDDaftarAwam + "E"));
                opt.AddPolicy(Modules.kodDDaftarAwam + "D", policy => policy.RequireClaim(Modules.kodDDaftarAwam + "D"));
                opt.AddPolicy(Modules.kodDDaftarAwam + "R", policy => policy.RequireClaim(Modules.kodDDaftarAwam + "R"));

                opt.AddPolicy(Modules.kodDKonfigKelulusan, policy => policy.RequireClaim(Modules.kodDKonfigKelulusan));
                opt.AddPolicy(Modules.kodDKonfigKelulusan + "C", policy => policy.RequireClaim(Modules.kodDKonfigKelulusan + "C"));
                opt.AddPolicy(Modules.kodDKonfigKelulusan + "E", policy => policy.RequireClaim(Modules.kodDKonfigKelulusan + "E"));
                opt.AddPolicy(Modules.kodDKonfigKelulusan + "D", policy => policy.RequireClaim(Modules.kodDKonfigKelulusan + "D"));
                opt.AddPolicy(Modules.kodDKonfigKelulusan + "R", policy => policy.RequireClaim(Modules.kodDKonfigKelulusan + "R"));

                opt.AddPolicy(Modules.kodDPanjar, policy => policy.RequireClaim(Modules.kodDPanjar));
                opt.AddPolicy(Modules.kodDPanjar + "C", policy => policy.RequireClaim(Modules.kodDPanjar + "C"));
                opt.AddPolicy(Modules.kodDPanjar + "E", policy => policy.RequireClaim(Modules.kodDPanjar + "E"));
                opt.AddPolicy(Modules.kodDPanjar + "D", policy => policy.RequireClaim(Modules.kodDPanjar + "D"));
                opt.AddPolicy(Modules.kodDPanjar + "R", policy => policy.RequireClaim(Modules.kodDPanjar + "R"));

                opt.AddPolicy(Modules.kodDPekerja, policy => policy.RequireClaim(Modules.kodDPekerja));
                opt.AddPolicy(Modules.kodDPekerja + "C", policy => policy.RequireClaim(Modules.kodDPekerja + "C"));
                opt.AddPolicy(Modules.kodDPekerja + "E", policy => policy.RequireClaim(Modules.kodDPekerja + "E"));
                opt.AddPolicy(Modules.kodDPekerja + "D", policy => policy.RequireClaim(Modules.kodDPekerja + "D"));
                opt.AddPolicy(Modules.kodDPekerja + "R", policy => policy.RequireClaim(Modules.kodDPekerja + "R"));

                opt.AddPolicy(Modules.kodDPenerimaCekGaji, policy => policy.RequireClaim(Modules.kodDPenerimaCekGaji));
                opt.AddPolicy(Modules.kodDPenerimaCekGaji + "C", policy => policy.RequireClaim(Modules.kodDPenerimaCekGaji + "C"));
                opt.AddPolicy(Modules.kodDPenerimaCekGaji + "E", policy => policy.RequireClaim(Modules.kodDPenerimaCekGaji + "E"));
                opt.AddPolicy(Modules.kodDPenerimaCekGaji + "D", policy => policy.RequireClaim(Modules.kodDPenerimaCekGaji + "D"));
                opt.AddPolicy(Modules.kodDPenerimaCekGaji + "R", policy => policy.RequireClaim(Modules.kodDPenerimaCekGaji + "R"));
                //

                // Akaun (AK)
                opt.AddPolicy(Modules.kodAbWaran, policy => policy.RequireClaim(Modules.kodAbWaran));
                opt.AddPolicy(Modules.kodAbWaran + "C", policy => policy.RequireClaim(Modules.kodAbWaran + "C"));
                opt.AddPolicy(Modules.kodAbWaran + "E", policy => policy.RequireClaim(Modules.kodAbWaran + "E"));
                opt.AddPolicy(Modules.kodAbWaran + "D", policy => policy.RequireClaim(Modules.kodAbWaran + "D"));
                opt.AddPolicy(Modules.kodAbWaran + "R", policy => policy.RequireClaim(Modules.kodAbWaran + "R"));
                opt.AddPolicy(Modules.kodAbWaran + "L", policy => policy.RequireClaim(Modules.kodAbWaran + "L"));
                opt.AddPolicy(Modules.kodAbWaran + "BL", policy => policy.RequireClaim(Modules.kodAbWaran + "BL"));

                opt.AddPolicy(Modules.kodAkBank, policy => policy.RequireClaim(Modules.kodAkBank));
                opt.AddPolicy(Modules.kodAkBank + "C", policy => policy.RequireClaim(Modules.kodAkBank + "C"));
                opt.AddPolicy(Modules.kodAkBank + "E", policy => policy.RequireClaim(Modules.kodAkBank + "E"));
                opt.AddPolicy(Modules.kodAkBank + "D", policy => policy.RequireClaim(Modules.kodAkBank + "D"));
                opt.AddPolicy(Modules.kodAkBank + "R", policy => policy.RequireClaim(Modules.kodAkBank + "R"));

                opt.AddPolicy(Modules.kodAkBelian, policy => policy.RequireClaim(Modules.kodAkBelian));
                opt.AddPolicy(Modules.kodAkBelian + "C", policy => policy.RequireClaim(Modules.kodAkBelian + "C"));
                opt.AddPolicy(Modules.kodAkBelian + "E", policy => policy.RequireClaim(Modules.kodAkBelian + "E"));
                opt.AddPolicy(Modules.kodAkBelian + "D", policy => policy.RequireClaim(Modules.kodAkBelian + "D"));
                opt.AddPolicy(Modules.kodAkBelian + "R", policy => policy.RequireClaim(Modules.kodAkBelian + "R"));
                opt.AddPolicy(Modules.kodAkBelian + "L", policy => policy.RequireClaim(Modules.kodAkBelian + "L"));
                opt.AddPolicy(Modules.kodAkBelian + "BL", policy => policy.RequireClaim(Modules.kodAkBelian + "BL"));

                opt.AddPolicy(Modules.kodAkCV, policy => policy.RequireClaim(Modules.kodAkCV));
                opt.AddPolicy(Modules.kodAkCV + "C", policy => policy.RequireClaim(Modules.kodAkCV + "C"));
                opt.AddPolicy(Modules.kodAkCV + "E", policy => policy.RequireClaim(Modules.kodAkCV + "E"));
                opt.AddPolicy(Modules.kodAkCV + "D", policy => policy.RequireClaim(Modules.kodAkCV + "D"));
                opt.AddPolicy(Modules.kodAkCV + "R", policy => policy.RequireClaim(Modules.kodAkCV + "R"));
                opt.AddPolicy(Modules.kodAkCV + "L", policy => policy.RequireClaim(Modules.kodAkCV + "L"));
                opt.AddPolicy(Modules.kodAkCV + "BL", policy => policy.RequireClaim(Modules.kodAkCV + "BL"));

                opt.AddPolicy(Modules.kodAkEFTMaybank2E, policy => policy.RequireClaim(Modules.kodAkEFTMaybank2E));
                opt.AddPolicy(Modules.kodAkEFTMaybank2E + "C", policy => policy.RequireClaim(Modules.kodAkEFTMaybank2E + "C"));
                opt.AddPolicy(Modules.kodAkEFTMaybank2E + "E", policy => policy.RequireClaim(Modules.kodAkEFTMaybank2E + "E"));
                opt.AddPolicy(Modules.kodAkEFTMaybank2E + "D", policy => policy.RequireClaim(Modules.kodAkEFTMaybank2E + "D"));
                opt.AddPolicy(Modules.kodAkEFTMaybank2E + "R", policy => policy.RequireClaim(Modules.kodAkEFTMaybank2E + "R"));

                opt.AddPolicy(Modules.kodAkInden, policy => policy.RequireClaim(Modules.kodAkInden));
                opt.AddPolicy(Modules.kodAkInden + "C", policy => policy.RequireClaim(Modules.kodAkInden + "C"));
                opt.AddPolicy(Modules.kodAkInden + "E", policy => policy.RequireClaim(Modules.kodAkInden + "E"));
                opt.AddPolicy(Modules.kodAkInden + "D", policy => policy.RequireClaim(Modules.kodAkInden + "D"));
                opt.AddPolicy(Modules.kodAkInden + "R", policy => policy.RequireClaim(Modules.kodAkInden + "R"));
                opt.AddPolicy(Modules.kodAkInden + "L", policy => policy.RequireClaim(Modules.kodAkInden + "L"));
                opt.AddPolicy(Modules.kodAkInden + "BL", policy => policy.RequireClaim(Modules.kodAkInden + "BL"));

                opt.AddPolicy(Modules.kodAkJanaanProfil, policy => policy.RequireClaim(Modules.kodAkJanaanProfil));
                opt.AddPolicy(Modules.kodAkJanaanProfil + "C", policy => policy.RequireClaim(Modules.kodAkJanaanProfil + "C"));
                opt.AddPolicy(Modules.kodAkJanaanProfil + "E", policy => policy.RequireClaim(Modules.kodAkJanaanProfil + "E"));
                opt.AddPolicy(Modules.kodAkJanaanProfil + "D", policy => policy.RequireClaim(Modules.kodAkJanaanProfil + "D"));
                opt.AddPolicy(Modules.kodAkJanaanProfil + "R", policy => policy.RequireClaim(Modules.kodAkJanaanProfil + "R"));

                opt.AddPolicy(Modules.kodAkJurnal, policy => policy.RequireClaim(Modules.kodAkJurnal));
                opt.AddPolicy(Modules.kodAkJurnal + "C", policy => policy.RequireClaim(Modules.kodAkJurnal + "C"));
                opt.AddPolicy(Modules.kodAkJurnal + "E", policy => policy.RequireClaim(Modules.kodAkJurnal + "E"));
                opt.AddPolicy(Modules.kodAkJurnal + "D", policy => policy.RequireClaim(Modules.kodAkJurnal + "D"));
                opt.AddPolicy(Modules.kodAkJurnal + "R", policy => policy.RequireClaim(Modules.kodAkJurnal + "R"));
                opt.AddPolicy(Modules.kodAkJurnal + "L", policy => policy.RequireClaim(Modules.kodAkJurnal + "L"));
                opt.AddPolicy(Modules.kodAkJurnal + "BL", policy => policy.RequireClaim(Modules.kodAkJurnal + "BL"));

                opt.AddPolicy(Modules.kodAkNotaMinta, policy => policy.RequireClaim(Modules.kodAkNotaMinta));
                opt.AddPolicy(Modules.kodAkNotaMinta + "C", policy => policy.RequireClaim(Modules.kodAkNotaMinta + "C"));
                opt.AddPolicy(Modules.kodAkNotaMinta + "E", policy => policy.RequireClaim(Modules.kodAkNotaMinta + "E"));
                opt.AddPolicy(Modules.kodAkNotaMinta + "D", policy => policy.RequireClaim(Modules.kodAkNotaMinta + "D"));
                opt.AddPolicy(Modules.kodAkNotaMinta + "R", policy => policy.RequireClaim(Modules.kodAkNotaMinta + "R"));

                opt.AddPolicy(Modules.kodAkPelarasanInden, policy => policy.RequireClaim(Modules.kodAkPelarasanInden));
                opt.AddPolicy(Modules.kodAkPelarasanInden + "C", policy => policy.RequireClaim(Modules.kodAkPelarasanInden + "C"));
                opt.AddPolicy(Modules.kodAkPelarasanInden + "E", policy => policy.RequireClaim(Modules.kodAkPelarasanInden + "E"));
                opt.AddPolicy(Modules.kodAkPelarasanInden + "D", policy => policy.RequireClaim(Modules.kodAkPelarasanInden + "D"));
                opt.AddPolicy(Modules.kodAkPelarasanInden + "R", policy => policy.RequireClaim(Modules.kodAkPelarasanInden + "R"));
                opt.AddPolicy(Modules.kodAkPelarasanInden + "L", policy => policy.RequireClaim(Modules.kodAkPelarasanInden + "L"));
                opt.AddPolicy(Modules.kodAkPelarasanInden + "BL", policy => policy.RequireClaim(Modules.kodAkPelarasanInden + "BL"));

                opt.AddPolicy(Modules.kodAkPelarasanPO, policy => policy.RequireClaim(Modules.kodAkPelarasanPO));
                opt.AddPolicy(Modules.kodAkPelarasanPO + "C", policy => policy.RequireClaim(Modules.kodAkPelarasanPO + "C"));
                opt.AddPolicy(Modules.kodAkPelarasanPO + "E", policy => policy.RequireClaim(Modules.kodAkPelarasanPO + "E"));
                opt.AddPolicy(Modules.kodAkPelarasanPO + "D", policy => policy.RequireClaim(Modules.kodAkPelarasanPO + "D"));
                opt.AddPolicy(Modules.kodAkPelarasanPO + "R", policy => policy.RequireClaim(Modules.kodAkPelarasanPO + "R"));
                opt.AddPolicy(Modules.kodAkPelarasanPO + "L", policy => policy.RequireClaim(Modules.kodAkPelarasanPO + "L"));
                opt.AddPolicy(Modules.kodAkPelarasanPO + "BL", policy => policy.RequireClaim(Modules.kodAkPelarasanPO + "BL"));

                opt.AddPolicy(Modules.kodAkPenilaianPerolehan, policy => policy.RequireClaim(Modules.kodAkPenilaianPerolehan));
                opt.AddPolicy(Modules.kodAkPenilaianPerolehan + "C", policy => policy.RequireClaim(Modules.kodAkPenilaianPerolehan + "C"));
                opt.AddPolicy(Modules.kodAkPenilaianPerolehan + "E", policy => policy.RequireClaim(Modules.kodAkPenilaianPerolehan + "E"));
                opt.AddPolicy(Modules.kodAkPenilaianPerolehan + "D", policy => policy.RequireClaim(Modules.kodAkPenilaianPerolehan + "D"));
                opt.AddPolicy(Modules.kodAkPenilaianPerolehan + "R", policy => policy.RequireClaim(Modules.kodAkPenilaianPerolehan + "R"));
                opt.AddPolicy(Modules.kodAkPenilaianPerolehan + "L", policy => policy.RequireClaim(Modules.kodAkPenilaianPerolehan + "L"));
                opt.AddPolicy(Modules.kodAkPenilaianPerolehan + "BL", policy => policy.RequireClaim(Modules.kodAkPenilaianPerolehan + "BL"));

                opt.AddPolicy(Modules.kodAkPO, policy => policy.RequireClaim(Modules.kodAkPO));
                opt.AddPolicy(Modules.kodAkPO + "C", policy => policy.RequireClaim(Modules.kodAkPO + "C"));
                opt.AddPolicy(Modules.kodAkPO + "E", policy => policy.RequireClaim(Modules.kodAkPO + "E"));
                opt.AddPolicy(Modules.kodAkPO + "D", policy => policy.RequireClaim(Modules.kodAkPO + "D"));
                opt.AddPolicy(Modules.kodAkPO + "R", policy => policy.RequireClaim(Modules.kodAkPO + "R"));
                opt.AddPolicy(Modules.kodAkPO + "L", policy => policy.RequireClaim(Modules.kodAkPO + "L"));
                opt.AddPolicy(Modules.kodAkPO + "BL", policy => policy.RequireClaim(Modules.kodAkPO + "BL"));

                opt.AddPolicy(Modules.kodAkPV, policy => policy.RequireClaim(Modules.kodAkPV));
                opt.AddPolicy(Modules.kodAkPV + "C", policy => policy.RequireClaim(Modules.kodAkPV + "C"));
                opt.AddPolicy(Modules.kodAkPV + "E", policy => policy.RequireClaim(Modules.kodAkPV + "E"));
                opt.AddPolicy(Modules.kodAkPV + "D", policy => policy.RequireClaim(Modules.kodAkPV + "D"));
                opt.AddPolicy(Modules.kodAkPV + "R", policy => policy.RequireClaim(Modules.kodAkPV + "R"));
                opt.AddPolicy(Modules.kodAkPV + "L", policy => policy.RequireClaim(Modules.kodAkPV + "L"));
                opt.AddPolicy(Modules.kodAkPV + "BL", policy => policy.RequireClaim(Modules.kodAkPV + "BL"));

                opt.AddPolicy(Modules.kodAkTerima, policy => policy.RequireClaim(Modules.kodAkTerima));
                opt.AddPolicy(Modules.kodAkTerima + "C", policy => policy.RequireClaim(Modules.kodAkTerima + "C"));
                opt.AddPolicy(Modules.kodAkTerima + "E", policy => policy.RequireClaim(Modules.kodAkTerima + "E"));
                opt.AddPolicy(Modules.kodAkTerima + "D", policy => policy.RequireClaim(Modules.kodAkTerima + "D"));
                opt.AddPolicy(Modules.kodAkTerima + "R", policy => policy.RequireClaim(Modules.kodAkTerima + "R"));
                opt.AddPolicy(Modules.kodAkTerima + "L", policy => policy.RequireClaim(Modules.kodAkTerima + "L"));
                opt.AddPolicy(Modules.kodAkTerima + "BL", policy => policy.RequireClaim(Modules.kodAkTerima + "BL"));

                opt.AddPolicy(Modules.kodAkNotaDebitKreditDiterima, policy => policy.RequireClaim(Modules.kodAkNotaDebitKreditDiterima));
                opt.AddPolicy(Modules.kodAkNotaDebitKreditDiterima + "C", policy => policy.RequireClaim(Modules.kodAkNotaDebitKreditDiterima + "C"));
                opt.AddPolicy(Modules.kodAkNotaDebitKreditDiterima + "E", policy => policy.RequireClaim(Modules.kodAkNotaDebitKreditDiterima + "E"));
                opt.AddPolicy(Modules.kodAkNotaDebitKreditDiterima + "D", policy => policy.RequireClaim(Modules.kodAkNotaDebitKreditDiterima + "D"));
                opt.AddPolicy(Modules.kodAkNotaDebitKreditDiterima + "R", policy => policy.RequireClaim(Modules.kodAkNotaDebitKreditDiterima + "R"));
                opt.AddPolicy(Modules.kodAkNotaDebitKreditDiterima + "L", policy => policy.RequireClaim(Modules.kodAkNotaDebitKreditDiterima + "L"));
                opt.AddPolicy(Modules.kodAkNotaDebitKreditDiterima + "BL", policy => policy.RequireClaim(Modules.kodAkNotaDebitKreditDiterima + "BL"));

                opt.AddPolicy(Modules.kodAkInvois, policy => policy.RequireClaim(Modules.kodAkInvois));
                opt.AddPolicy(Modules.kodAkInvois + "C", policy => policy.RequireClaim(Modules.kodAkInvois + "C"));
                opt.AddPolicy(Modules.kodAkInvois + "E", policy => policy.RequireClaim(Modules.kodAkInvois + "E"));
                opt.AddPolicy(Modules.kodAkInvois + "D", policy => policy.RequireClaim(Modules.kodAkInvois + "D"));
                opt.AddPolicy(Modules.kodAkInvois + "R", policy => policy.RequireClaim(Modules.kodAkInvois + "R"));
                opt.AddPolicy(Modules.kodAkInvois + "L", policy => policy.RequireClaim(Modules.kodAkInvois + "L"));
                opt.AddPolicy(Modules.kodAkInvois + "BL", policy => policy.RequireClaim(Modules.kodAkInvois + "BL"));
                //

                // Pemprosesan (PP)
                opt.AddPolicy(Modules.kodLulusAbWaran + "L", policy => policy.RequireClaim(Modules.kodLulusAbWaran + "L"));
                opt.AddPolicy(Modules.kodSahAbWaran + "S", policy => policy.RequireClaim(Modules.kodSahAbWaran + "S"));
                opt.AddPolicy(Modules.kodSemakAbWaran + "S", policy => policy.RequireClaim(Modules.kodSemakAbWaran + "S"));
                opt.AddPolicy(Modules.kodLulusAbWaran + "E", policy => policy.RequireClaim(Modules.kodLulusAbWaran + "E"));

                opt.AddPolicy(Modules.kodLulusAkBelian + "L", policy => policy.RequireClaim(Modules.kodLulusAkBelian + "L"));
                opt.AddPolicy(Modules.kodLulusAkBelian + "E", policy => policy.RequireClaim(Modules.kodLulusAkBelian + "E"));

                opt.AddPolicy(Modules.kodLulusAkInden + "L", policy => policy.RequireClaim(Modules.kodLulusAkInden + "L"));
                opt.AddPolicy(Modules.kodLulusAkInden + "E", policy => policy.RequireClaim(Modules.kodLulusAkInden + "E"));

                opt.AddPolicy(Modules.kodLulusAkJurnal + "L", policy => policy.RequireClaim(Modules.kodLulusAkJurnal + "L"));
                opt.AddPolicy(Modules.kodSahAkJurnal + "S", policy => policy.RequireClaim(Modules.kodSahAkJurnal + "S"));
                opt.AddPolicy(Modules.kodSemakAkJurnal + "S", policy => policy.RequireClaim(Modules.kodSemakAkJurnal + "S"));
                opt.AddPolicy(Modules.kodLulusAkJurnal + "E", policy => policy.RequireClaim(Modules.kodLulusAkJurnal + "E"));

                opt.AddPolicy(Modules.kodLulusAkPelarasanInden + "L", policy => policy.RequireClaim(Modules.kodLulusAkPelarasanInden + "L"));
                opt.AddPolicy(Modules.kodLulusAkPelarasanInden + "E", policy => policy.RequireClaim(Modules.kodLulusAkPelarasanInden + "E"));

                opt.AddPolicy(Modules.kodLulusAkPelarasanPO + "L", policy => policy.RequireClaim(Modules.kodLulusAkPelarasanPO + "L"));
                opt.AddPolicy(Modules.kodLulusAkPelarasanPO + "E", policy => policy.RequireClaim(Modules.kodLulusAkPelarasanPO + "E"));

                opt.AddPolicy(Modules.kodLulusAkPenilaianPerolehan + "L", policy => policy.RequireClaim(Modules.kodLulusAkPenilaianPerolehan + "L"));
                opt.AddPolicy(Modules.kodSahAkPenilaianPerolehan + "S", policy => policy.RequireClaim(Modules.kodSahAkPenilaianPerolehan + "S"));
                opt.AddPolicy(Modules.kodSemakAkPenilaianPerolehan + "S", policy => policy.RequireClaim(Modules.kodSemakAkPenilaianPerolehan + "S"));
                opt.AddPolicy(Modules.kodLulusAkPenilaianPerolehan + "E", policy => policy.RequireClaim(Modules.kodLulusAkPenilaianPerolehan + "E"));

                opt.AddPolicy(Modules.kodLulusAkNotaMinta + "L", policy => policy.RequireClaim(Modules.kodLulusAkNotaMinta + "L"));
                opt.AddPolicy(Modules.kodSahAkNotaMinta + "S", policy => policy.RequireClaim(Modules.kodSahAkNotaMinta + "S"));
                opt.AddPolicy(Modules.kodSemakAkNotaMinta + "S", policy => policy.RequireClaim(Modules.kodSemakAkNotaMinta + "S"));
                opt.AddPolicy(Modules.kodLulusAkNotaMinta + "E", policy => policy.RequireClaim(Modules.kodLulusAkNotaMinta + "E"));

                opt.AddPolicy(Modules.kodLulusAkPO + "L", policy => policy.RequireClaim(Modules.kodLulusAkPO + "L"));
                opt.AddPolicy(Modules.kodLulusAkPO + "E", policy => policy.RequireClaim(Modules.kodLulusAkPO + "E"));

                opt.AddPolicy(Modules.kodLulusAkPV + "L", policy => policy.RequireClaim(Modules.kodLulusAkPV + "L"));
                opt.AddPolicy(Modules.kodLulusAkPV + "E", policy => policy.RequireClaim(Modules.kodLulusAkPV + "E"));

                opt.AddPolicy(Modules.kodLulusAkNotaDebitKreditDiterima + "L", policy => policy.RequireClaim(Modules.kodLulusAkNotaDebitKreditDiterima + "L"));
                opt.AddPolicy(Modules.kodLulusAkNotaDebitKreditDiterima + "E", policy => policy.RequireClaim(Modules.kodLulusAkNotaDebitKreditDiterima + "E"));

                opt.AddPolicy(Modules.kodLulusAkInvois + "L", policy => policy.RequireClaim(Modules.kodLulusAkInvois + "L"));
                opt.AddPolicy(Modules.kodLulusAkInvois + "E", policy => policy.RequireClaim(Modules.kodLulusAkInvois + "E"));
                //


            }
            );
            return services;
        }
    }
}
