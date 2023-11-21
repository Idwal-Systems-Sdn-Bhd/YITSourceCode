using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YIT.__Domain.Entities.Models._03Akaun;

namespace YIT._DataAccess.Services.Cart
{
    public class CartAkInden
    {
        //IndenObjek
        private List<AkIndenObjek> collectionObjek = new List<AkIndenObjek>();

        public virtual void AddItemObjek(
            int akIndenId,
            int jKWPTJBahagianId,
            int akCartaId,
            decimal amaun
            )
        {
            AkIndenObjek line = collectionObjek.FirstOrDefault(pp => pp.JKWPTJBahagianId == jKWPTJBahagianId && pp.AkCartaId == akCartaId)!;

            if (line == null)
            {
                collectionObjek.Add(new AkIndenObjek()
                {
                    AkIndenId = akIndenId,
                    JKWPTJBahagianId = jKWPTJBahagianId,
                    AkCartaId = akCartaId,
                    Amaun = amaun
                });
            }
        }

        public virtual void RemoveItemObjek(int jKWPTJBahagianId, int akCartaId) => collectionObjek.RemoveAll(l => l.JKWPTJBahagianId == jKWPTJBahagianId && l.AkCartaId == akCartaId);

        public virtual void ClearObjek() => collectionObjek.Clear();

        public virtual IEnumerable<AkIndenObjek> AkIndenObjek => collectionObjek;
        //

        // IndenPerihal

        private List<AkIndenPerihal> collectionPerihal = new List<AkIndenPerihal>();
        public virtual void AddItemPerihal(
            int akIndenId,
            decimal bil,
            string? perihal,
            decimal kuantiti,
            string? unit,
            decimal harga,
            decimal amaun
            )
        {
            AkIndenPerihal line = collectionPerihal.FirstOrDefault(pp => pp.Bil == bil)!;

            if (line == null)
            {
                collectionPerihal.Add(new AkIndenPerihal
                {
                    AkIndenId = akIndenId,
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

        public virtual IEnumerable<AkIndenPerihal> AkIndenPerihal => collectionPerihal;
        //
    }
}
