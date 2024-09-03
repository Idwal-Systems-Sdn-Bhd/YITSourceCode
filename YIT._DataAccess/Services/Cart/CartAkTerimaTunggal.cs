using YIT.__Domain.Entities._Enums;
using YIT.__Domain.Entities.Models._03Akaun;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YIT._DataAccess.Services.Cart
{
    public class CartAkTerimaTunggal
    {
        //TerimaTunggalObjek

        private List<AkTerimaTunggalObjek> collectionObjek = new List<AkTerimaTunggalObjek>();

        public virtual void AddItemObjek(
            int akTerimaTunggalId,
            int jKWPTJBahagianId,
            int akCartaId,
            decimal amaun
            )
        {
            AkTerimaTunggalObjek line = collectionObjek.FirstOrDefault(p => p.JKWPTJBahagianId == jKWPTJBahagianId && p.AkCartaId == akCartaId)!;

            if (line == null)
            {
                collectionObjek.Add(new AkTerimaTunggalObjek
                {
                    AkTerimaTunggalId = akTerimaTunggalId,
                    JKWPTJBahagianId = jKWPTJBahagianId,
                    AkCartaId = akCartaId,
                    Amaun = amaun
                });
            }
        }

        public virtual void RemoveItemObjek(int jKWPTJBahagianId, int akCartaId) =>
            collectionObjek.RemoveAll(l => l.AkCartaId == akCartaId && l.JKWPTJBahagianId == jKWPTJBahagianId);


        public virtual void ClearObjek() => collectionObjek.Clear();

        public virtual IEnumerable<AkTerimaTunggalObjek> AkTerimaTunggalObjek => collectionObjek;

        //TerimaTunggalInvois

        private List<AkTerimaTunggalInvois> collectionInvois = new List<AkTerimaTunggalInvois>();

        public virtual void AddItemInvois(
            int akTerimaTunggalId,
            int akInvoisId,
            decimal amaun
            )
        {
            AkTerimaTunggalInvois line = collectionInvois.FirstOrDefault(p => p.AkInvoisId == akInvoisId)!;

            if (line == null)
            {
                collectionInvois.Add(new AkTerimaTunggalInvois
                {
                    AkTerimaTunggalId = akTerimaTunggalId,
                    AkInvoisId = akInvoisId,
                    Amaun = amaun
                });
            }
        }

        public virtual void RemoveItemInvois(int akInvoisId) =>
            collectionInvois.RemoveAll(l => l.AkInvoisId == akInvoisId);


        public virtual void ClearInvois() => collectionInvois.Clear();

        public virtual IEnumerable<AkTerimaTunggalInvois> AkTerimaTunggalInvois => collectionInvois;
    }
}
