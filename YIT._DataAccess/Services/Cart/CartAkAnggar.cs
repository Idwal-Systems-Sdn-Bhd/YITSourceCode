using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YIT.__Domain.Entities.Models._03Akaun;

namespace YIT._DataAccess.Services.Cart
{
    public class CartAkAnggar
    {
        private List<AkAnggarObjek> collectionObjek = new List<AkAnggarObjek>();

    public virtual void AddItemObjek(
        int akAnggarId,
        int jKWPTJBahagianId,
        int akCartaId,
        decimal amaun
        )
    {
        AkAnggarObjek line = collectionObjek.FirstOrDefault(p => p.JKWPTJBahagianId == jKWPTJBahagianId && p.AkCartaId == akCartaId)!;

        if (line == null)
        {
            collectionObjek.Add(new AkAnggarObjek
            {
                AkAnggarId = akAnggarId,
                JKWPTJBahagianId = jKWPTJBahagianId,
                AkCartaId = akCartaId,
                Amaun = amaun,
            });
        }
    }

    public virtual void RemoveItemObjek(int jKWPTJBahagianId, int akCartaId) =>
        collectionObjek.RemoveAll(l => l.AkCartaId == akCartaId && l.JKWPTJBahagianId == jKWPTJBahagianId);


    public virtual void ClearObjek() => collectionObjek.Clear();

    public virtual IEnumerable<AkAnggarObjek> AkAnggarObjek => collectionObjek;


}
}
