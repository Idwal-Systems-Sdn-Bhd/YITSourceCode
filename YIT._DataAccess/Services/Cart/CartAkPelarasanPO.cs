using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YIT.__Domain.Entities.Models._03Akaun;

namespace YIT._DataAccess.Services.Cart
{
    public class CartAkPelarasanPO
    {
        //AkPelarasanPOObjek
        private List<AkPelarasanPOObjek> collectionObjek = new List<AkPelarasanPOObjek>();

        public virtual void AddItemObjek(
            int akPelarasanPOId,
            int jKWPTJBahagianId,
            int akCartaId,
            decimal amaun
            )
        {
            AkPelarasanPOObjek line = collectionObjek.FirstOrDefault(pp => pp.JKWPTJBahagianId == jKWPTJBahagianId && pp.AkCartaId == akCartaId)!;

            if (line == null)
            {
                collectionObjek.Add(new AkPelarasanPOObjek()
                {
                    AkPelarasanPOId = akPelarasanPOId,
                    JKWPTJBahagianId = jKWPTJBahagianId,
                    AkCartaId = akCartaId,
                    Amaun = amaun
                });
            }
        }

        public virtual void RemoveItemObjek(int jKWPTJBahagianId, int akCartaId) => collectionObjek.RemoveAll(l => l.JKWPTJBahagianId == jKWPTJBahagianId && l.AkCartaId == akCartaId);

        public virtual void ClearObjek() => collectionObjek.Clear();

        public virtual IEnumerable<AkPelarasanPOObjek> AkPelarasanPOObjek => collectionObjek;
        //

        // AkPelarasanPOPerihal

        private List<AkPelarasanPOPerihal> collectionPerihal = new List<AkPelarasanPOPerihal>();
        public virtual void AddItemPerihal(
            int akPelarasanPOId,
            decimal bil,
            string? perihal,
            decimal kuantiti,
            string? unit,
            decimal harga,
            decimal amaun
            )
        {
            AkPelarasanPOPerihal line = collectionPerihal.FirstOrDefault(pp => pp.Bil == bil)!;

            if (line == null)
            {
                collectionPerihal.Add(new AkPelarasanPOPerihal
                {
                    AkPelarasanPOId = akPelarasanPOId,
                    Bil = bil,
                    Perihal = perihal,
                    Kuantiti = kuantiti,
                    Unit = unit,
                    Harga = harga,
                    Amaun = amaun

                });
            }
        }

        public virtual void RemoveItemPerihal(decimal bil) => collectionPerihal.RemoveAll(l => l.Bil == bil);

        public virtual void ClearPerihal() => collectionPerihal.Clear();

        public virtual IEnumerable<AkPelarasanPOPerihal> AkPelarasanPOPerihal => collectionPerihal;
        //
    }
}
