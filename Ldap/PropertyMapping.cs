namespace EmploAdImport.Ldap
{
    public class PropertyMapping
    {
        public PropertyMapping(string emploPropertyName, string ldapPropertyName)
        {
            EmploPropertyName =  emploPropertyName;
            LdapPropertyName = ldapPropertyName;
        }
        
        public string EmploPropertyName { get; private set; }
        public string LdapPropertyName { get; private set; }
    }
}
