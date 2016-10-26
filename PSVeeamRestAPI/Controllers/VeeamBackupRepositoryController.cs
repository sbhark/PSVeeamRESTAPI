using NLog;
using PSVeeamRESTAPI.App_Start;
using PSVeeamRESTAPI.Models;
using PSVeeamRESTAPI.Services;
using System;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Web.Http;

namespace PSVeeamRESTAPI.Controllers
{
    [RoutePrefix("api/veeam/{vbrHost}/backupRepos")]
    public class VeeamBackupRepositoryController : ApiController
    {
        private static VeeamPowerShellAgent psAgent = new VeeamPowerShellAgent();
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static Configs config = new Configs();

        [Route("{repoName}")]
        [HttpGet]
        [PSVeeamRESTAPI.Filters.Authorization]
        public IHttpActionResult GetBackupRepository(string vbrHost, string repoName)
        {
            logger.Info("Received request to retrieve Veeam Backup Repository Info for " + repoName + " from VBR " + vbrHost);

            string command = "$repo = Get-VBRBackupRepository -Name '" + repoName + "'; $repoInfo = $repo.Info; $repo; $repoInfo;";
            VeeamTransportMessage response = psAgent.runCommand(vbrHost, command);

            if (response.status.Equals("Error"))
            {
                return BadRequest((String)response.message);
            }

            Collection<PSObject> veeamBackupRepo = (Collection<PSObject>)response.message;

            // Quick check if a valid backup repo name was specified
            if (veeamBackupRepo[0] == null)
            {
                return NotFound();
            }

            VeeamBackupRepository backupRepo = new VeeamBackupRepository
            {
                id = veeamBackupRepo[0].Properties["Id"].Value.ToString(),
                name = veeamBackupRepo[0].Properties["Name"].Value.ToString(),
                description = veeamBackupRepo[0].Properties["Description"].Value.ToString(),
                path = veeamBackupRepo[1].Properties["Path"].Value.ToString(),
                capacity = Int64.Parse(veeamBackupRepo[1].Properties["CachedTotalSpace"].Value.ToString()),
                freeSpace = Int64.Parse(veeamBackupRepo[1].Properties["CachedFreeSpace"].Value.ToString()),
            };

            return Ok(backupRepo);
        }
    }
}
