using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YIT.__Domain.Entities.Models._03Akaun;

namespace YIT._DataAccess.Services.Cart
{
    public class CartAkPelarasanInden
    {
        //AkPelarasanIndenObjek
        private List<AkPelarasanIndenObjek> collectionObjek = new List<AkPelarasanIndenObjek>();

        public virtual void AddItemObjek(
            int akPelarasanIndenId,
            int jKWPTJBahagianId,
            int akCartaId,
            decimal amaun
            )
        {
            AkPelarasanIndenObjek line = collectionObjek.FirstOrDefault(pp => pp.JKWPTJBahagianId == jKWPTJBahagianId && pp.AkCartaId == akCartaId)!;

            if (line == null)
            {
                collectionObjek.Add(new AkPelarasanIndenObjek()
                {
                    AkPelarasanIndenId = akPelarasanIndenId,
                    JKWPTJBahagianId = jKWPTJBahagianId,
                    AkCartaId = akCartaId,
                    Amaun = amaun
                });
            }
        }

        public virtual void RemoveItemObjek(int jKWPTJBahagianId, int akCartaId) => collectionObjek.RemoveAll(l => l.JKWPTJBahagianId == jKWPTJBahagianId && l.AkCartaId == akCartaId);

        public virtual void ClearObjek() => collectionObjek.Clear();

        public virtual IEnumerable<AkPelarasanIndenObjek> AkPelarasanIndenObjek => collectionObjek;
        //

        // AkPelarasanIndenPerihal

        private List<AkPelarasanIndenPerihal> collectionPerihal = new List<AkPelarasanIndenPerihal>();
        public virtual void AddItemPerihal(
            int akPelarasanIndenId,
            decimal bil,
            string? perihal,
            decimal kuantiti,
            string? unit,
            decimal harga,
            decimal amaun
            )
        {
            AkPelarasanIndenPerihal line = collectionPerihal.FirstOrDefault(pp => pp.Bil == bil)!;

            if (line == null)
            {
                collectionPerihal.Add(new AkPelarasanIndenPerihal
                {
                    AkPelarasanIndenId = akPelarasanIndenId,
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

        public virtual IEnumerable<AkPelarasanIndenPerihal> AkPelarasanIndenPerihal => collectionPerihal;
        //
    }
}
