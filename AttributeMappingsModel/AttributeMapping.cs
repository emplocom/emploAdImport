using System.Configuration;

namespace EmploAdImport.AttributeMappingsModel
{
    public class AttributeMapping : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new AttributeMappingElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((AttributeMappingElement)element).Name;
        }
    }
}