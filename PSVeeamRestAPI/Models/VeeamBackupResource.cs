namespace PSVeeamRESTAPI.Models
{
    public class VeeamBackupResource
    {
        public string uid { get; set; }
        public string backupRepositoryName { get; set; }
        public string repositoryDisplayName { get; set; }
        public string repositoryPath { get; set; }
        public int repositoryQuota { get; set; }
        public int usedSpace { get; set; }
        public double usedPercentage { get; set; }
        public bool enableWanAccelerator { get; set; }
        public string wanAcceleratorName { get; set; }
    }
}