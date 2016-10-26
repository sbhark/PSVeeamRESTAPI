using System.Configuration;

namespace PSVeeamRESTAPI.AppConfigurations
{
    public class VeeamPSProxySection : ConfigurationSection
    {
        [ConfigurationProperty("proxys")]
        public VeeamPSProxiesCollection proxys
        {
            get { return ((VeeamPSProxiesCollection)(base["proxys"])); }
            set { base["proxys"] = value; }
        }

    }
}