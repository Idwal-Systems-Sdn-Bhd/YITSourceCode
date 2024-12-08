﻿using YIT.__Domain.Entities.Models._03Akaun;


namespace YIT.Akaun.Models.ViewModels.Prints
{
    public class LAK002PrintModel
    {
        public CommonPrintModel CommonModels { get; set; } = new CommonPrintModel();
        public List<AkPV>? AkPV { get; set; }
        public List<AkJurnal>? AkJurnal { get; set; }
    }
}