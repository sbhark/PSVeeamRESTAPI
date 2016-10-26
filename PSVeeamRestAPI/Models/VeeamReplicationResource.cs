namespace PSVeeamRESTAPI.Models
{
    public class VeeamReplicationResource
    {
        public string hwPlanOptionUid { get; set; }
        public string hwPlanUid {get; set;}
        public string hwPlanName { get; set; }
        public string platform { get; set; }
        public int cpuQuota { get; set; }
        public int cpuUsed { get; set; }
        public int memoryQuota { get; set; }
        public int memoryUsed { get; set; }
        public int netWithInternet { get; set; }
        public int netWithoutInternet { get; set; }
        public string datastoreName { get; set; }
        public string datastoreDisplayName { get; set; }
        public int datastoreQuota { get; set; }
        public int datastoreUsed { get; set; }
        public int publicIpCount { get; set; }
        public string wanAcceleratorId { get; set; }
    }
}