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

        public const string superAdminRole = "SuperAdmin";

        public const string adminRole = "Admin";
        public const string supervisorRole = "Supervisor";
        public const string userRole = "User";

        public const string superAdminAdminRole = superAdminRole + "," + adminRole;
        public const string superAdminSupervisorRole = superAdminRole + "," + supervisorRole;
        public const string allExceptAdminRole = superAdminRole + "," + supervisorRole + "," + userRole;
        public const string allExceptSuperadminRole = adminRole + "," + supervisorRole + "," + userRole;
        public const string allRole = superAdminRole + "," + supervisorRole + "," + userRole + "," + adminRole;

        // initial password
        public const string commonPassword = "Spmb1234#";

        // initial Company Details
        public const string CompSystemCode = "SPMB";
        public const string CompName = "Yayasan Islam Terengganu";
        public const string CompInitial = "YIT";
        public const string CompRegNo = "000586846208";
        public const string CompAddress1 = "Tingkat 9,10 & 11";
        public const string CompAddress2 = "Menara Yayasan Islam Terengganu";
        public const string CompAddress3 = "Jalan Sultan Omar";
        public const string CompCity = "Kuala Terengganu";
        public const string CompPoscode = "20300";
        public const string CompDistrict = "Kuala Terengganu";
        public const string CompState = "Terengganu";
        public const string CompTel = "+09-6300 300/301/302/303";
        public const string CompEmail = "admin@yit.gov.my";
        public const string CompFax = "+09 622 6511";
        public const string CompWebLogo = "MainLogo_Syarikat.webp";
        public const string CompPrintLogo = "MainLogo_Syarikat.png";
    }
}
