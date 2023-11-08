using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YIT.__Domain.Entities._Statics
{
    public static class Init
    {
        public const string superAdminName = "SuperAdmin";
        public const string superAdminEmail = "superadmin@idwal.com.my";
        public const string superAdminPassword = "SuperIdwalsys57#";

        public const string superAdminAuthRole = "SuperAdmin";

        public const string adminRole = "Admin";
        public const string superVisorRole = "Supervisor";
        public const string userRole = "User";

        public const string superAdminAdminAuthRole = superAdminAuthRole + "," + adminRole;
        public const string superAdminSuperVisorAuthRole = superAdminAuthRole + "," + superVisorRole;
        public const string allExceptAdminRole = superAdminAuthRole + "," + superVisorRole + "," + userRole;
        public const string allExceptSuperadminRole = adminRole + "," + superVisorRole + "," + userRole;
        public const string allRole = superAdminAuthRole + "," + superVisorRole + "," + userRole + "," + adminRole;

        // initial password
        public const string commonPassword = "Spmb1234#";

        // initial Company Details
        public const string CompSystemCode = "SPMB";
        public const string CompName = "Idwal Systems Sdn Bhd";
        public const string CompRegNo = "187842-T";
        public const string CompAddress1 = "Lot 605G";
        public const string CompAddress2 = "Kompleks Diamond, Bangi Business Park";
        public const string CompAddress3 = "Jalan Medan Bangi, Off Persiaran Bandar";
        public const string CompCity = "Bandar Baru Bangi";
        public const string CompPoscode = "43640";
        public const string CompDistrict = "Ulu Langat";
        public const string CompState = "Selangor";
        public const string CompTel = "+6011 33272978";
        public const string CompEmail = "far@idwal.com.my";
        public const string CompFax = "";
        public const string CompWebLogo = "MainLogo_Syarikat.webp";
        public const string CompPrintLogo = "MainLogo_Syarikat.png";
    }
}
