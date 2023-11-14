using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YIT.__Domain.Entities.Models._03Akaun;

namespace YIT._DataAccess.Services.Cart
{
    public class CartAkPenilaianPerolehan
    {
        //PenilaianPerolehanObjek
        private List<AkPenilaianPerolehanObjek> collectionObjek = new List<AkPenilaianPerolehanObjek>();

        public virtual void AddItemObjek(
            int akPenilaianPerolehanId,
            int jBahagianId,
            int akCartaId,
            decimal amaun
            )
        {
            AkPenilaianPerolehanObjek line = collectionObjek.FirstOrDefault(pp => pp.JBahagianId == jBahagianId && pp.AkCartaId == akCartaId)!;

            if ( line == null )
            {
                collectionObjek.Add(new AkPenilaianPerolehanObjek()
                {
                    AkPenilaianPerolehanId = akPenilaianPerolehanId,
                    JBahagianId = jBahagianId,
                    AkCartaId = akCartaId,
                    Amaun = amaun
                });
            }
        }

        public virtual void RemoveItemObjek(int jBahagianId, int akCartaId) => collectionObjek.RemoveAll(l => l.JBahagianId == jBahagianId && l.AkCartaId == akCartaId);

        public virtual void ClearObjek() => collectionObjek.Clear();

        public virtual IEnumerable<AkPenilaianPerolehanObjek> AkPenilaianPerolehanObjek => collectionObjek;
        //

        // PenilaianPerolehanPerihal

        private List<AkPenilaianPerolehanPerihal>  collectionPerihal = new List<AkPenilaianPerolehanPerihal>();
        public virtual void AddItemPerihal(
            int akPenilaianPerolehanId,
            decimal bil,
            string? perihal,
            decimal kuantiti,
            string? unit,
            decimal harga,
            decimal amaun
            )
        {
            AkPenilaianPerolehanPerihal line = collectionPerihal.FirstOrDefault(pp => pp.Bil == bil)!;

            if ( line == null )
            {
                collectionPerihal.Add(new AkPenilaianPerolehanPerihal
                {
                    AkPenilaianPerolehanId = akPenilaianPerolehanId,
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

        public virtual IEnumerable<AkPenilaianPerolehanPerihal> AkPenilaianPerolehanPerihal => collectionPerihal;
        //
    }
}
