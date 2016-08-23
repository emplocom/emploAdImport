using System.Configuration;

namespace EmploAdImport.AttributeMappingsModel
{
    public class AttributeMappingElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsKey = true, IsRequired = true)]
        public string Name
        {
            get { return (string)base["name"]; }
            set { base["name"] = value; }
        }

        [ConfigurationProperty("value", IsRequired = false)]
        public string Value
        {
            get { return (string)base["value"]; }
            set { base["value"] = value; }
        }
    }
}