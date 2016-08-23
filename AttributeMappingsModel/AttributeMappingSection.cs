using System.Configuration;

namespace EmploAdImport.AttributeMappingsModel
{
    public class AttributeMappingSection : ConfigurationSection
    {
        public const string SectionName = "AttributeMappingSection";
        private const string EndpointCollectionName = "AttributeMapping";

        [ConfigurationProperty(EndpointCollectionName)]
        [ConfigurationCollection(typeof(AttributeMapping), AddItemName = "add")]
        public AttributeMapping Instances
        {
            get
            {
                return (AttributeMapping)base[EndpointCollectionName];
            }
        }
    }
}
