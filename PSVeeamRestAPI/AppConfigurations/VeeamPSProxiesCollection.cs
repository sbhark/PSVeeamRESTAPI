using System;
using System.Configuration;

namespace PSVeeamRESTAPI.AppConfigurations
{
    [ConfigurationCollection(typeof(VeeamPSProxy))]
    public class VeeamPSProxiesCollection : ConfigurationElementCollection
    {
        internal const string PropertyName = "proxy";

        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.BasicMapAlternate;
            }
        }

        protected override string ElementName
        {
            get
            {
                return PropertyName;
            }
        }

        protected override bool IsElementName(string elementName)
        {
            return elementName.Equals(PropertyName, StringComparison.InvariantCultureIgnoreCase);
        }

        public override bool IsReadOnly()
        {
            return false;
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new VeeamPSProxy();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((VeeamPSProxy)(element)).hostNameOrIp;
        }

        public VeeamPSProxy this[int idx]
        {
            get { return (VeeamPSProxy)BaseGet(idx); }
        }
    }
}