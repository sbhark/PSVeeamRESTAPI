namespace PSVeeamRESTAPI.Models
{
    public class VeeamTenant
    {
        public string uid { get; set; }
        public string username { get; set; }
	    public string password { get; set; }
	    public string leaseExpiration { get; set; }
    }
}