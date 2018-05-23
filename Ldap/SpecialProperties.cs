using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmploAdImport.Ldap
{
    public static class SpecialProperties
    {
        public const string FirstOrganizationalUnit = "$(firstOrganizationalUnit)";
        public const string OrganizationalUnitsList = "$(organizationalUnitsList)";
    }
}
