using YIT.__Domain.Entities._Statics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YIT.__Domain.Entities.Administrations
{
    public class CompanyDetails
    {
        public string KodSistem { get; set; }
        public DateTime TarVersi { get; set; }
        public string NamaSyarikat { get; set; }
        public string NoPendaftaran { get; set; }
        public string AlamatSyarikat1 { get; set; }
        public string AlamatSyarikat2 { get; set; }
        public string AlamatSyarikat3 { get; set; }
        public string Bandar { get; set; }
        public string Poskod { get; set; }
        public string Daerah { get; set; }
        public string Negeri { get; set; }
        public string TelSyarikat { get; set; }
        public string FaksSyarikat { get; set; }
        public string EmelSyarikat { get; set; }
        public DateTime TarMula { get; set; }
        public string CompanyLogoWeb { get; set; }
        public string CompanyLogoPrintPDF { get; set; }


        public CompanyDetails()
        {
            KodSistem = Init.CompSystemCode;
            NamaSyarikat = Init.CompName;
            NoPendaftaran = Init.CompRegNo;
            AlamatSyarikat1 = Init.CompAddress1;
            AlamatSyarikat2 = Init.CompAddress2;
            AlamatSyarikat3 = Init.CompAddress3;
            Bandar = Init.CompCity;
            Poskod = Init.CompPoscode;
            Daerah = Init.CompDistrict;
            Negeri = Init.CompState;
            TelSyarikat = Init.CompTel;
            FaksSyarikat = Init.CompFax;
            EmelSyarikat = Init.CompEmail;
            CompanyLogoWeb = Init.CompWebLogo;
            CompanyLogoPrintPDF = Init.CompPrintLogo;

        }
    }
}
