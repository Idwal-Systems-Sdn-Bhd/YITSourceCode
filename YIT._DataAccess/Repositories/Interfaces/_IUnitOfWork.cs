using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YIT._DataAccess.Repositories.Implementations;

namespace YIT._DataAccess.Repositories.Interfaces
{
    public interface _IUnitOfWork : IDisposable
    {
        IDPekerjaRepository DPekerjaRepo { get; }
        IDDaftarAwamRepository DDaftarAwamRepo { get; }
        IDKonfigKelulusanRepository DKonfigKelulusanRepo { get; }
        IDPenerimaCekGajiRepository DPenerimaCekGajiRepo { get; }
        IJKWRepository JKWRepo { get; }
        IJPTJRepository JPTJRepo { get; }
        IJAgamaRepository JAgamaRepo { get; }
        IJBangsaRepository JBangsaRepo { get; }
        IJBankRepository JBankRepo { get; }
        IJCaraBayarRepository JCaraBayarRepo { get; }
        IJNegeriRepository JNegeriRepo { get; }
        IJBahagianRepository JBahagianRepo { get; }
        IJCukaiRepository JCukaiRepo { get; }
        IAkCartaRepository AkCartaRepo { get; }
        IAkBankRepository AkBankRepo { get; }
        IAkTerimaRepository AkTerimaRepo { get; }
        IJCawanganRepository JCawanganRepo { get; }
        IAbWaranRepository AbWaranRepo { get; }
        IAkAnggarRepository AkAnggarRepo { get; }
        IAkPenilaianPerolehanRepository AkPenilaianPerolehanRepo { get; }
        IAkPORepository AkPORepo { get; }
        IAkNotaMintaRepository AkNotaMintaRepo { get; }
        IAkIndenRepository AkIndenRepo { get; }
        IJKWPTJBahagianRepository JKWPTJBahagianRepo { get; }
        IAkPelarasanPORepository AkPelarasanPORepo { get; }
        IAkPelarasanIndenRepository AkPelarasanIndenRepo { get; }
        IAkBelianRepository AkBelianRepo { get; }
        IAkPVRepository AkPVRepo { get; }
        IAkRekupRepository AkRekupRepo { get; }

        IAkJanaanProfilRepository AkJanaanProfilRepo { get; }
        IAkEFTRepository AkEFTRepo { get; }
        IAkJurnalRepository AkJurnalRepo { get; }
        IJKonfigPerubahanEkuitiRepository JKonfigPerubahanEkuitiRepo { get; }
        IJKonfigPenyataRepository JKonfigPenyataRepo { get; }
        IDPanjarRepository DPanjarRepo { get; }
        IAkCVRepository AkCVRepo { get; }
        IAkInvoisRepository AkInvoisRepo { get; }
        ILHDNKodKlasifikasiRepository LHDNKodKlasifikasiRepo { get; }
        ILHDNMSICRepository LHDNMSICRepo { get; }
        ILHDNUnitUkuranRepository LHDNUnitUkuranRepo { get; }
        IAkNotaDebitKreditDiterimaRepository AkNotaDebitKreditDiterimaRepo { get; }
        IAkNotaDebitKreditDikeluarkanRepository AkNotaDebitKreditDikeluarkanRepo { get; }
        IAkTerimaTunggalRepository AkTerimaTunggalRepo { get; }
        Task<int> Save();
    }
}
