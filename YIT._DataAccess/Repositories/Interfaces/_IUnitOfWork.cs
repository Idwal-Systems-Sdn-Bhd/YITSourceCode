using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YIT._DataAccess.Repositories.Interfaces
{
    public interface _IUnitOfWork : IDisposable
    {
        IDPekerjaRepository DPekerjaRepo { get; }
        IDDaftarAwamRepository DDaftarAwamRepo { get; }
        IDPenyemakRepository DPenyemakRepo { get; }
        IDPelulusRepository DPelulusRepo { get; }
        IJKWRepository JKWRepo { get; }
        IJPTJRepository JPTJRepo { get; }
        IJAgamaRepository JAgamaRepo { get; }
        IJBangsaRepository JBangsaRepo { get; }
        IJBankRepository JBankRepo { get; }
        IJCaraBayarRepository JCaraBayarRepo { get; }
        IJNegeriRepository JNegeriRepo { get; }
        IJBahagianRepository JBahagianRepo { get; }
        IAkCartaRepository AkCartaRepo { get; }
        IAkBankRepository AkBankRepo { get; }
        IAkTerimaRepository AkTerimaRepo { get; }
        int Save();
    }
}
