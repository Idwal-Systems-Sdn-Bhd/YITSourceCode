using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YIT.__Domain.Entities.Models._02Daftar;

namespace YIT._DataAccess.Services.Cart
{
    public class CartDPanjar
    {
        // DPanjarPemegang
        private List<DPanjarPemegang> collectionPemegang = new List<DPanjarPemegang>();

        public virtual void AddItemPemegang(
            int dPanjarId,
            int dPekerjaId,
            DateTime jangkaMasaDari,
            DateTime? jangkaMasaHingga,
            bool isAktif)
        {
            DPanjarPemegang line = collectionPemegang.FirstOrDefault(p => p.DPekerjaId == dPekerjaId)!;

                if (line == null)
            {
                collectionPemegang.Add(new DPanjarPemegang()
                {
                    DPanjarId = dPanjarId,
                    DPekerjaId = dPekerjaId,
                    JangkaMasaDari = jangkaMasaDari,
                    JangkaMasaHingga = jangkaMasaHingga,
                    IsAktif = isAktif
                });
            }
        }

        public virtual void RemoveItemPemegang(int dPekerjaId) => collectionPemegang.RemoveAll(p => p.DPekerjaId == dPekerjaId);
        
        public virtual void ClearPemegang() => collectionPemegang.Clear();

        public virtual IEnumerable<DPanjarPemegang> DPanjarPemegang => collectionPemegang;
        // DPanjarPemegang end
    }
}
