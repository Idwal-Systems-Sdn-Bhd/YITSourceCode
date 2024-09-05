using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YIT.__Domain.Entities.Models._03Akaun;

namespace YIT._DataAccess.Services.Cart
{
    public class CartAkPenyataPemungut
    {
        private List<AkPenyataPemungutObjek> collectionObjek = new List<AkPenyataPemungutObjek>();

        public virtual void AddItemObjek(
            int akPenyataPemungutId,
            int akTerimaTunggalId,
            decimal bil,
            int jKWPTJBahagianId,
            int akCartaId,
            decimal amaun
            )
        {
            AkPenyataPemungutObjek line = collectionObjek.FirstOrDefault(pp => pp.Bil == bil)!;

            if (line == null)
            {
                collectionObjek.Add(new AkPenyataPemungutObjek
                {
                    AkPenyataPemungutId = akPenyataPemungutId,
                    AkTerimaTunggalId = akTerimaTunggalId,
                    Bil = bil,
                    JKWPTJBahagianId = jKWPTJBahagianId,
                    AkCartaId = akCartaId,
                    Amaun = amaun
                });
            }
        }

        public virtual void RemoveItemObjek(decimal bil)
        {
            AkPenyataPemungutObjek line = collectionObjek.FirstOrDefault(pp => pp.Bil == bil)!;

            if (line != null)
            {
                collectionObjek.RemoveAll(pp => pp.AkTerimaTunggalId == line.AkTerimaTunggalId);
            }
        }

        public virtual void ClearObjek() => collectionObjek.Clear();

        public virtual IEnumerable<AkPenyataPemungutObjek> AkPenyataPemungutObjek => collectionObjek;
    }
}
