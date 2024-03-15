using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YIT.__Domain.Entities._Enums;
using YIT.__Domain.Entities.Models._01Jadual;

namespace YIT._DataAccess.Services.Cart
{
    public class CartJKonfigPenyata
    {
        // baris
        private List<JKonfigPenyataBaris> collectionBaris = new List<JKonfigPenyataBaris>();
        
        public virtual void AddItemBaris(
            int bil,
            int jKonfigPenyataId,
            EnKategoriTajuk enKategoriTajuk,
            string? perihal,
            int susunan,
            bool isFormula,
            EnKategoriJumlah enKategoriJumlah,
            string? jumlahSusunanList,
            List<JKonfigPenyataBarisFormula> jKonfigPenyataBarisFormulas)
        {
            JKonfigPenyataBaris line = collectionBaris.FirstOrDefault(b => b.Bil == bil)!;

            if (line == null)
            {
                collectionBaris.Add(new JKonfigPenyataBaris
                {
                    Bil = bil,
                    JKonfigPenyataId = jKonfigPenyataId,
                    EnKategoriTajuk = enKategoriTajuk,
                    Perihal = perihal,
                    Susunan = susunan,
                    IsFormula = isFormula,
                    EnKategoriJumlah = enKategoriJumlah,
                    JumlahSusunanList = jumlahSusunanList,
                    JKonfigPenyataBarisFormula = jKonfigPenyataBarisFormulas
                });
            }
        }

        public virtual void RemoveItemBaris(int bil) => collectionBaris.RemoveAll(b => b.Bil == bil);

        public virtual void ClearBaris() => collectionBaris.Clear();

        public virtual IEnumerable<JKonfigPenyataBaris> JKonfigPenyataBaris => collectionBaris;
        // baris end

        // baris formula
        private List<JKonfigPenyataBarisFormula> collectionBarisFormula = new List<JKonfigPenyataBarisFormula>();

        public virtual void AddItemBarisFormula(
            int barisBil,
            int jKonfigPenyataBarisId,
            EnJenisOperasi enJenisOperasi,
            bool isPukal,
            string? enJenisCartaList,
            bool isKecuali,
            string? kodList,
            string? setKodList
            )
        {
            JKonfigPenyataBarisFormula line = collectionBarisFormula.FirstOrDefault(b => b.EnJenisOperasi == enJenisOperasi && (b.BarisBil == barisBil))!;

            if (line == null)
            {
                collectionBarisFormula.Add(new JKonfigPenyataBarisFormula
                {
                    BarisBil = barisBil,
                    JKonfigPenyataBarisId = jKonfigPenyataBarisId,
                    EnJenisOperasi = enJenisOperasi,
                    IsPukal = isPukal,
                    EnJenisCartaList = enJenisCartaList,
                    IsKecuali = isKecuali,
                    KodList = kodList,
                    SetKodList = setKodList
                });
            }
        }

        public virtual void RemoveItemBarisFormula(EnJenisOperasi enJenisOperasi, int barisBil) => collectionBarisFormula.RemoveAll(b => b.EnJenisOperasi == enJenisOperasi && b.BarisBil == b.BarisBil);

        public virtual void ClearBarisFormulaByBarisBil() => collectionBarisFormula.Clear();

        public virtual void ClearBarisFormula() => collectionBarisFormula.Clear();

        public virtual IEnumerable<JKonfigPenyataBarisFormula> JKonfigPenyataBarisFormula => collectionBarisFormula;


        // baris formula end
    }
}
