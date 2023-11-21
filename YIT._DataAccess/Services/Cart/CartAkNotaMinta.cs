using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YIT.__Domain.Entities.Models._03Akaun;

namespace YIT._DataAccess.Services.Cart
{
    public class CartAkNotaMinta
    {
        //NotaMintaObjek
        private List<AkNotaMintaObjek> collectionObjek = new List<AkNotaMintaObjek>();

        public virtual void AddItemObjek(
            int akNotaMintaId,
            int jKWPTJBahagianId,
            int akCartaId,
            decimal amaun
            )
        {
            AkNotaMintaObjek line = collectionObjek.FirstOrDefault(pp => pp.JKWPTJBahagianId == jKWPTJBahagianId && pp.AkCartaId == akCartaId)!;

            if (line == null)
            {
                collectionObjek.Add(new AkNotaMintaObjek()
                {
                    AkNotaMintaId = akNotaMintaId,
                    JKWPTJBahagianId = jKWPTJBahagianId,
                    AkCartaId = akCartaId,
                    Amaun = amaun
                });
            }
        }

        public virtual void RemoveItemObjek(int jKWPTJBahagianId, int akCartaId) => collectionObjek.RemoveAll(l => l.JKWPTJBahagianId == jKWPTJBahagianId && l.AkCartaId == akCartaId);

        public virtual void ClearObjek() => collectionObjek.Clear();

        public virtual IEnumerable<AkNotaMintaObjek> AkNotaMintaObjek => collectionObjek;
        //

        // NotaMintaPerihal

        private List<AkNotaMintaPerihal> collectionPerihal = new List<AkNotaMintaPerihal>();
        public virtual void AddItemPerihal(
            int akNotaMintaId,
            decimal bil,
            string? perihal,
            decimal kuantiti,
            string? unit,
            decimal harga,
            decimal amaun
            )
        {
            AkNotaMintaPerihal line = collectionPerihal.FirstOrDefault(pp => pp.Bil == bil)!;

            if (line == null)
            {
                collectionPerihal.Add(new AkNotaMintaPerihal
                {
                    AkNotaMintaId = akNotaMintaId,
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

        public virtual IEnumerable<AkNotaMintaPerihal> AkNotaMintaPerihal => collectionPerihal;
        //
    }
}
