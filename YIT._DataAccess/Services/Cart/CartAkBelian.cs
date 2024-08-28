using YIT.__Domain.Entities._Enums;
using YIT.__Domain.Entities.Models._03Akaun;

namespace YIT._DataAccess.Services.Cart
{
    public class CartAkBelian
    {
        //BelianObjek
        private List<AkBelianObjek> collectionObjek = new List<AkBelianObjek>();

        public virtual void AddItemObjek(
            int akBelianId,
            int jKWPTJBahagianId,
            int akCartaId,
            int? jCukaiId,
            decimal amaun
            )
        {
            AkBelianObjek line = collectionObjek.FirstOrDefault(pp => pp.JKWPTJBahagianId == jKWPTJBahagianId && pp.AkCartaId == akCartaId)!;

            if (line == null)
            {
                collectionObjek.Add(new AkBelianObjek()
                {
                    AkBelianId = akBelianId,
                    JKWPTJBahagianId = jKWPTJBahagianId,
                    AkCartaId = akCartaId,
                    JCukaiId = jCukaiId,
                    Amaun = amaun
                });
            }
        }

        public virtual void RemoveItemObjek(int jKWPTJBahagianId, int akCartaId) => collectionObjek.RemoveAll(l => l.JKWPTJBahagianId == jKWPTJBahagianId && l.AkCartaId == akCartaId);

        public virtual void ClearObjek() => collectionObjek.Clear();

        public virtual IEnumerable<AkBelianObjek> AkBelianObjek => collectionObjek;
        //

        // BelianPerihal

        private List<AkBelianPerihal> collectionPerihal = new List<AkBelianPerihal>();
        public virtual void AddItemPerihal(
            int akBelianId,
            decimal bil,
            string? perihal,
            decimal kuantiti,
            int? lHDNKodKlasifikasiId,
            int? lHDNUnitUkuranId,
            string? unit,
            EnLHDNJenisCukai enLHDNJenisCukai,
            decimal kadarCukai,
            decimal amaunCukai,
            decimal harga,
            decimal amaun
            )
        {
            AkBelianPerihal line = collectionPerihal.FirstOrDefault(pp => pp.Bil == bil)!;

            if (line == null)
            {
                amaunCukai = Math.PriceFormatter.GetTaxAmount(harga, kuantiti, kadarCukai);
                collectionPerihal.Add(new AkBelianPerihal
                {
                    AkBelianId = akBelianId,
                    Bil = bil,
                    Perihal = perihal,
                    Kuantiti = kuantiti,
                    LHDNKodKlasifikasiId = lHDNKodKlasifikasiId,
                    LHDNUnitUkuranId = lHDNUnitUkuranId,
                    Unit = unit,
                    EnLHDNJenisCukai = enLHDNJenisCukai,
                    KadarCukai = kadarCukai,
                    AmaunCukai = amaunCukai,
                    Harga = harga,
                    Amaun = Math.PriceFormatter.GetTotalPayableAmount(harga, kuantiti, amaunCukai)

                });
            }
        }

        public virtual void RemoveItemPerihal(decimal bil) => collectionPerihal.RemoveAll(l => l.Bil == bil);

        public virtual void ClearPerihal() => collectionPerihal.Clear();

        public virtual IEnumerable<AkBelianPerihal> AkBelianPerihal => collectionPerihal;
        //
    }
}
