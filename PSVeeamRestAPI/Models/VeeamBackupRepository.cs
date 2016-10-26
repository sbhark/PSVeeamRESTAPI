
namespace PSVeeamRESTAPI.Models
{
    public class VeeamBackupRepository
    {
        public string id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string path { get; set; }
        public long capacity { get; set; }
        public long freeSpace { get; set; }
    }
}