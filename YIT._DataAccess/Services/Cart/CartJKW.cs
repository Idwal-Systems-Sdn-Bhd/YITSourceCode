using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YIT.__Domain.Entities.Models._01Jadual;

namespace YIT._DataAccess.Services.Cart
{
    public class CartJKW
    {
        private List<JKWPTJBahagian> collectionList = new List<JKWPTJBahagian>();

        public virtual void AddItemList(
            int jKWId,
            int jPTJId,
            int jBahagianId,
            string? kod
            )
        {
            JKWPTJBahagian line = collectionList.FirstOrDefault(p => p.JKWId == jKWId && p.JPTJId == jPTJId && p.JBahagianId == jBahagianId)!;

            if (line == null )
            {
                collectionList.Add(
                    new JKWPTJBahagian
                    {
                        JKWId = jKWId,
                        JPTJId = jPTJId,
                        JBahagianId = jBahagianId,
                        Kod = kod
                    }
                    );
            }
        }

        public virtual void RemoveItemList(int jKWId, int jPTJId, int jBahagianId) => collectionList.RemoveAll(l => l.JKWId == jKWId && l.JPTJId == jPTJId && l.JBahagianId == jBahagianId);

        public virtual void ClearList() => collectionList.Clear();

        public virtual IEnumerable<JKWPTJBahagian> JKWPTJBahagian => collectionList;
    }
}
