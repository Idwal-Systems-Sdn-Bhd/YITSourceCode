using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YIT.__Domain.Entities._Enums;
using YIT.__Domain.Entities.Models._03Akaun;

namespace YIT._DataAccess.Services.Cart
{
    public class CartAbWaran
    {
        private List<AbWaranObjek> collectionObjek = new List<AbWaranObjek>();

        public virtual void AddItemObjek(
            int abWaranId,
            int jBahagianId,
            int akCartaId,
            decimal amaun,
            string? TK
            )
        {
            AbWaranObjek line = collectionObjek.FirstOrDefault(p => p.JBahagianId == jBahagianId && p.AkCartaId == akCartaId)!;

            if (line == null)
            {
                collectionObjek.Add(new AbWaranObjek
                {
                    AbWaranId = abWaranId,
                    JBahagianId = jBahagianId,
                    AkCartaId = akCartaId,
                    Amaun = amaun,
                    TK = TK
                });
            }
        }

        public virtual void RemoveItemObjek(int jBahagianId, int akCartaId) =>
            collectionObjek.RemoveAll(l => l.AkCartaId == akCartaId && l.JBahagianId == jBahagianId);


        public virtual void ClearObjek() => collectionObjek.Clear();

        public virtual IEnumerable<AbWaranObjek> abWaranObjek => collectionObjek;


    }
}
