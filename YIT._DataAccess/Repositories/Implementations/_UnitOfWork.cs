using YIT.__Domain.Entities.Models._03Akaun;
using YIT._DataAccess.Data;
using YIT._DataAccess.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YIT._DataAccess.Repositories.Implementations
{
    public class _UnitOfWork : _IUnitOfWork
    {
        private readonly ApplicationDbContext _context;

        public _UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            DPekerjaRepo = new DPekerjaRepository(_context);
            DDaftarAwamRepo = new DDaftarAwamRepository(_context);
            DKonfigKelulusanRepo = new DKonfigKelulusanRepository(_context);
            DPenerimaCekGajiRepo = new DPenerimaCekGajiRepository(_context);

            JKWRepo = new JKWRepository(_context);
            JPTJRepo = new JPTJRepository(_context);
            JAgamaRepo = new JAgamaRepository(_context);
            JBangsaRepo = new JBangsaRepository(_context);
            JBankRepo = new JBankRepository(_context);
            JCaraBayarRepo = new JCaraBayarRepository(_context);
            JNegeriRepo = new JNegeriRepository(_context);
            JBahagianRepo = new JBahagianRepository(_context);
            JCawanganRepo = new JCawanganRepository(_context);
            JKWPTJBahagianRepo = new JKWPTJBahagianRepository(context);
            JCukaiRepo = new JCukaiRepository(context);

            AkCartaRepo = new AkCartaRepository(_context);
            AkBankRepo = new AkBankRepository(_context);
            
            AkTerimaRepo = new AkTerimaRepository(_context);

            AbWaranRepo = new AbWaranRepository(_context);

            AkPenilaianPerolehanRepo = new AkPenilaianPerolehanRepository(_context);
            AkNotaMintaRepo = new AkNotaMintaRepository(_context);

            AkPORepo = new AkPORepository(_context);
            AkIndenRepo = new AkIndenRepository(_context);

            AkPelarasanPORepo = new AkPelarasanPORepository(_context);
            AkPelarasanIndenRepo = new AkPelarasanIndenRepository(_context);

            AkBelianRepo = new AkBelianRepository(_context);

            AkPVRepo = new AkPVRepository(_context);
            AkJanaanProfilRepo = new AkJanaanProfilRepository(_context);
            AkEFTRepo = new AkEFTRepository(_context);

            AkJurnalRepo = new AkJurnalRepository(_context);
            JKonfigPerubahanEkuitiRepo = new JKonfigPerubahanEkuitiRepository(_context);
            JKonfigPenyataRepo = new JKonfigPenyataRepository(_context);

            AkRekupRepo = new AkRekupRepository(_context);
            DPanjarRepo = new DPanjarRepository(_context);
            AkCVRepo = new AkCVRepository(_context);
            AkInvoisRepo = new AkInvoisRepository(_context);

            LHDNKodKlasifikasiRepo = new LHDNKodKlasifikasiRepository(_context);
            LHDNMSICRepo = new LHDNMSICRepository(_context);
            LHDNUnitUkuranRepo = new LHDNUnitUkuranRepository(_context);

            AkNotaDebitKreditDiterimaRepo = new AkNotaDebitKreditDiterimaRepository(_context);
        }

        public IJKWRepository JKWRepo { get; private set; }

        public IDPekerjaRepository DPekerjaRepo { get; private set; }

        public IJPTJRepository JPTJRepo {get; private set; }

        public IJAgamaRepository JAgamaRepo { get; private set; }

        public IJBangsaRepository JBangsaRepo { get; private set; }

        public IJBankRepository JBankRepo { get; private set; }

        public IJCaraBayarRepository JCaraBayarRepo {  get; private set; }

        public IJNegeriRepository JNegeriRepo {  get; private set; }

        public IJBahagianRepository JBahagianRepo { get; private set; }
        public IJCawanganRepository JCawanganRepo { get; private set; }

        public IDKonfigKelulusanRepository DKonfigKelulusanRepo {get; private set; }

        public IDDaftarAwamRepository DDaftarAwamRepo {get; private set; }
        public IDPenerimaCekGajiRepository DPenerimaCekGajiRepo { get; private set; }

        public IAkCartaRepository AkCartaRepo {get; private set;}

        public IAkBankRepository AkBankRepo {get; private set; }

        public IAkTerimaRepository AkTerimaRepo {get; private set;}
        public IAbWaranRepository AbWaranRepo { get; private set; }

        public IAkPenilaianPerolehanRepository AkPenilaianPerolehanRepo { get; private set; }

        public IAkPORepository AkPORepo { get; private set; }

        public IAkNotaMintaRepository AkNotaMintaRepo { get; private set; }

        public IAkIndenRepository AkIndenRepo { get; private set; }

        public IJKWPTJBahagianRepository JKWPTJBahagianRepo { get; private set; }

        public IAkPelarasanPORepository AkPelarasanPORepo { get; private set; }

        public IAkPelarasanIndenRepository AkPelarasanIndenRepo { get; private set; }

        public IAkBelianRepository AkBelianRepo { get; }

        public IJCukaiRepository JCukaiRepo { get; }

        public IAkPVRepository AkPVRepo { get; }

        public IAkJanaanProfilRepository AkJanaanProfilRepo {  get; }

        public IAkEFTRepository AkEFTRepo { get; }

        public IAkJurnalRepository AkJurnalRepo { get; }

        public IJKonfigPerubahanEkuitiRepository JKonfigPerubahanEkuitiRepo { get; }

        public IJKonfigPenyataRepository JKonfigPenyataRepo { get; }

        public IAkRekupRepository AkRekupRepo { get; }

        public IDPanjarRepository DPanjarRepo { get; }

        public IAkCVRepository AkCVRepo { get; }

        public IAkInvoisRepository AkInvoisRepo { get; }

        public ILHDNKodKlasifikasiRepository LHDNKodKlasifikasiRepo { get; }

        public ILHDNMSICRepository LHDNMSICRepo { get; }

        public ILHDNUnitUkuranRepository LHDNUnitUkuranRepo { get; }

        public IAkNotaDebitKreditDiterimaRepository AkNotaDebitKreditDiterimaRepo { get; }

        public void Dispose()
        {
            _context.Dispose();
        }

        public async Task<int> Save()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
