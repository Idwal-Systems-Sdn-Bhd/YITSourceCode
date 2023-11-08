using YIT.__Domain.Entities._Enums;
using YIT.__Domain.Entities.Models._03Akaun;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YIT._DataAccess.Services.Cart
{
    public class CartAkTerima
    {
        //TerimaObjek

        private List<AkTerimaObjek> collectionObjek = new List<AkTerimaObjek>();

        public virtual void AddItemObjek(
            int akTerimaId,
            int jBahagianId,
            int akCartaId,
            decimal amaun
            )
        {
            AkTerimaObjek line = collectionObjek.FirstOrDefault(p => p.JBahagianId == jBahagianId && p.AkCartaId == akCartaId)!;

            if (line == null)
            {
                collectionObjek.Add(new AkTerimaObjek
                {
                    AkTerimaId = akTerimaId,
                    JBahagianId = jBahagianId,
                    AkCartaId = akCartaId,
                    Amaun = amaun
                });
            }
        }

        public virtual void RemoveItemObjek(int jBahagianId, int akCartaId) =>
            collectionObjek.RemoveAll(l => l.AkCartaId == akCartaId && l.JBahagianId == jBahagianId);


        public virtual void ClearObjek() => collectionObjek.Clear();

        public virtual IEnumerable<AkTerimaObjek> akTerimaObjek => collectionObjek;

        //TerimaCaraBayar

        private List<AkTerimaCaraBayar> collectionCaraBayar = new List<AkTerimaCaraBayar>();

        public virtual void AddItemCaraBayar(
            int akTerimaId,
            int jCaraBayarId,
            decimal amaun,
            string? noCekMK,
            EnJenisCek enJenisCek,
            string? kodBankCek,
            string? tempatCek,
            string? noSlip,
            DateTime? tarikhSlip
            )
        {
            AkTerimaCaraBayar line = collectionCaraBayar.FirstOrDefault(p => p.JCaraBayarId == jCaraBayarId)!;

            if (line == null)
            {
                collectionCaraBayar.Add(new AkTerimaCaraBayar
                {
                    AkTerimaId = akTerimaId,
                    JCaraBayarId = jCaraBayarId,
                    Amaun = amaun,
                    NoCekMK = noCekMK,
                    EnJenisCek = enJenisCek,
                    KodBankCek = kodBankCek,
                    TempatCek = tempatCek,
                    NoSlip = noSlip,
                    TarikhSlip = tarikhSlip

                });
            }
        }

        public virtual void RemoveItemCaraBayar(int jCaraBayarId) =>
            collectionCaraBayar.RemoveAll(l => l.JCaraBayarId == jCaraBayarId);


        public virtual void ClearCaraBayar() => collectionCaraBayar.Clear();

        public virtual IEnumerable<AkTerimaCaraBayar> akTerimaCaraBayar => collectionCaraBayar;
    }
}
