﻿using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;
using YIT.__Domain.Entities.Models._01Jadual;

namespace YIT.__Domain.Entities.Models._03Akaun
{
    public class AkPelarasanIndenObjek
    {
        public int Id { get; set; }
        public int AkPelarasanIndenId { get; set; }
        public AkPelarasanInden? AkPelarasanInden { get; set; }
        [DisplayName("Carta")]
        public int AkCartaId { get; set; }
        public AkCarta? AkCarta { get; set; }
        [DisplayName("Bahagian")]
        public int JKWPTJBahagianId { get; set; }
        public JKWPTJBahagian? JKWPTJBahagian { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Amaun { get; set; }
    }
}