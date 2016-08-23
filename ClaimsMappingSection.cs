using System.Configuration;

namespace EmploAdImport
{
    public class ClaimsMappingSection : ConfigurationSection
    {
        public const string SectionName = "ClaimsMappingSection";
        private const string EndpointCollectionName = "ClaimsMapping";

        [ConfigurationProperty(EndpointCollectionName)]
        [ConfigurationCollection(typeof(ClaimsMapping), AddItemName = "add")]
        public ClaimsMapping Instances
        {
            get
            {
                return (ClaimsMapping)base[EndpointCollectionName];
            }
        }
    }

    public class ClaimsMapping : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new ClaimsMappingElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ClaimsMappingElement)element).Name;
        }
    }

    public class ClaimsMappingElement : ConfigurationElement
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
