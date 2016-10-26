using System.Configuration;

namespace PSVeeamRESTAPI.AppConfigurations
{
    public class VeeamPSProxy : ConfigurationElement
    {
        [ConfigurationProperty("hostNameOrIp", IsRequired = true, IsKey = true)]
        public string hostNameOrIp
        {
            get { return(string) base["hostNameOrIp"];  }
        }

        [ConfigurationProperty("username", IsRequired = true)]
        public string username
        {
            get { return (string)base["username"]; }
        }

        [ConfigurationProperty("password", IsRequired = true)]
        public string password
        {
            get { return (string)base["password"]; }
        }
    }
}