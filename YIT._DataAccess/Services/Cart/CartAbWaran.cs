using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YIT.__Domain.Entities.Models._03Akaun;

namespace YIT._DataAccess.Services.Cart
{
    public class CartAbWaran
    {
        private List<AbWaranObjek> collectionObjek = new List<AbWaranObjek>();

        public virtual void AddItemObjek(
            int abWaranId,
            int jKWPTJBahagianId,
            int akCartaId,
            decimal amaun,
            string? tk
            )
        {
            AbWaranObjek line = collectionObjek.FirstOrDefault(p => p.JKWPTJBahagianId == jKWPTJBahagianId && p.AkCartaId == akCartaId)!;

            if (line == null)
            {
                collectionObjek.Add(new AbWaranObjek
                {
                    AbWaranId = abWaranId,
                    JKWPTJBahagianId = jKWPTJBahagianId,
                    AkCartaId = akCartaId,
                    Amaun = amaun,
                    TK = tk
                });
            }
        }

        public virtual void RemoveItemObjek(int jKWPTJBahagianId, int akCartaId) =>
            collectionObjek.RemoveAll(l => l.AkCartaId == akCartaId && l.JKWPTJBahagianId == jKWPTJBahagianId);


        public virtual void ClearObjek() => collectionObjek.Clear();

        public virtual IEnumerable<AbWaranObjek> AbWaranObjek => collectionObjek;


    }
}
