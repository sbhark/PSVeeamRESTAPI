using System;

using System.Net;
using System.Net.Http;
using System.Web.Http;

using PSVeeamRESTAPI.Models;
using System.Management.Automation;
using System.Collections.ObjectModel;
using PSVeeamRESTAPI.Filters;
using PSVeeamRESTAPI.Services;
using NLog;
using PSVeeamRESTAPI.App_Start;


namespace PSVeeamRESTAPI.Controllers
{
    [RoutePrefix("api/veeam/{vbrHost}/tenants/{tenantUid}/backups")]
    public class VeeamBackupResourceController : ApiController
    {
        private static JSONSchemaValidator schemaValidator = new JSONSchemaValidator();
        private static VeeamPowerShellAgent psAgent = new VeeamPowerShellAgent();
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static Configs config = new Configs();

        public VeeamBackupResourceController(){ }

        [Route("")]
        [HttpGet]
        [PSVeeamRESTAPI.Filters.Authorization]
        public IHttpActionResult GetBackupResources(string vbrHost, string tenantUid)
        {
            logger.Info("Received request to get all Veeam Backup resources for tenant: " + tenantUid);

            String command = "Get-VBRCloudTenant -Id '" + tenantUid + "' | Select -ExpandProperty Resources";
            VeeamTransportMessage response = psAgent.runCommand(vbrHost, command);

            if (response.status.Equals("Error"))
            {
                return BadRequest((String)response.message);
            }

            Collection<PSObject> buRepos = (Collection<PSObject>)response.message;

            Collection<VeeamBackupResource> backupRepos = new Collection<VeeamBackupResource>();
            foreach (PSObject buRepo in buRepos)
            {
                VeeamBackupResource buResource = null;
                if (Boolean.Parse(buRepo.Properties["WanAccelerationEnabled"].Value.ToString()))
                {
                    buResource = new VeeamBackupResource
                    {
                        uid = buRepo.Properties["Id"].Value.ToString(),
                        repositoryDisplayName = buRepo.Properties["RepositoryFriendlyName"].Value.ToString(),
                        repositoryQuota = Int32.Parse(buRepo.Properties["RepositoryQuota"].Value.ToString()),
                        repositoryPath = buRepo.Properties["RepositoryQuotaPath"].Value.ToString(),
                        usedSpace = Int32.Parse(buRepo.Properties["UsedSpace"].Value.ToString()),
                        usedPercentage = Double.Parse(buRepo.Properties["UsedSpacePercentage"].Value.ToString()),
                        enableWanAccelerator = true,
                        wanAcceleratorName = buRepo.Properties["WanAccelerator"].Value.ToString()
                    };
                } else
                {
                    buResource = new VeeamBackupResource
                    {
                        uid = buRepo.Properties["Id"].Value.ToString(),
                        repositoryDisplayName = buRepo.Properties["RepositoryFriendlyName"].Value.ToString(),
                        repositoryQuota = Int32.Parse(buRepo.Properties["RepositoryQuota"].Value.ToString()),
                        repositoryPath = buRepo.Properties["RepositoryQuotaPath"].Value.ToString(),
                        usedSpace = Int32.Parse(buRepo.Properties["UsedSpace"].Value.ToString()),
                        usedPercentage = Double.Parse(buRepo.Properties["UsedSpacePercentage"].Value.ToString()),
                    };
                }

                backupRepos.Add(buResource);
            }

            return Ok(backupRepos);
        }

        [Route("{resourceUid}")]
        [HttpGet]
        [PSVeeamRESTAPI.Filters.Authorization]
        public IHttpActionResult GetBackupResource(string vbrHost, string tenantUid, string resourceUid)
        {
            logger.Info("Received request to get Veeam Backup resource: " + resourceUid + " for tenant: " + tenantUid);

            String command = "$cloudRepo = Get-VBRCloudTenant -Id '" + tenantUid +
                "' | Select -ExpandProperty Resources | Where-Object {$_.Id -eq '" +
                resourceUid + "'}; $buRepo = $cloudRepo.Repository.Name; $cloudRepo; $buRepo;";
            VeeamTransportMessage response = psAgent.runCommand(vbrHost, command);

            if (response.status.Equals("Error"))
            {
                return BadRequest((String)response.message);
            }

            Collection<PSObject> buRepo = (Collection<PSObject>)response.message;

            if (buRepo[0] == null)
            {
                return NotFound();
            }

            VeeamBackupResource buResource = null;
            if (Boolean.Parse(buRepo[0].Properties["WanAccelerationEnabled"].Value.ToString()))
            {
                buResource = new VeeamBackupResource
                {
                    uid = buRepo[0].Properties["Id"].Value.ToString(),
                    backupRepositoryName = buRepo[1].ToString(),
                    repositoryDisplayName = buRepo[0].Properties["RepositoryFriendlyName"].Value.ToString(),
                    repositoryQuota = Int32.Parse(buRepo[0].Properties["RepositoryQuota"].Value.ToString()),
                    repositoryPath = buRepo[0].Properties["RepositoryQuotaPath"].Value.ToString(),
                    usedSpace = Int32.Parse(buRepo[0].Properties["UsedSpace"].Value.ToString()),
                    usedPercentage = Double.Parse(buRepo[0].Properties["UsedSpacePercentage"].Value.ToString()),
                    enableWanAccelerator = true,
                    wanAcceleratorName = buRepo[0].Properties["WanAccelerator"].Value.ToString()
                };
            } else
            {
                buResource = new VeeamBackupResource
                {
                    uid = buRepo[0].Properties["Id"].Value.ToString(),
                    backupRepositoryName = buRepo[1].ToString(),
                    repositoryDisplayName = buRepo[0].Properties["RepositoryFriendlyName"].Value.ToString(),
                    repositoryQuota = Int32.Parse(buRepo[0].Properties["RepositoryQuota"].Value.ToString()),
                    repositoryPath = buRepo[0].Properties["RepositoryQuotaPath"].Value.ToString(),
                    usedSpace = Int32.Parse(buRepo[0].Properties["UsedSpace"].Value.ToString()),
                    usedPercentage = Double.Parse(buRepo[0].Properties["UssedSpacePercentage"].Value.ToString()),
                };
            }

            return Ok(buResource);
        }

        [Route("name/{resourceName}")]
        [HttpGet]
        [PSVeeamRESTAPI.Filters.Authorization]
        public IHttpActionResult GetBackupResourceByName(string vbrHost, string tenantUid, string resourceName)
        {
            logger.Info("Received request to get Veeam Backup resource: " + resourceName + " for tenant: " + tenantUid);

            String command = "$cloudRepo = Get-VBRCloudTenant -Id '" + tenantUid +
                "' | Select -ExpandProperty Resources | Where-Object {$_.RepositoryFriendlyName -eq '" +
                resourceName + "'}; $buRepo = $cloudRepo.Repository.Name; $cloudRepo; $buRepo;";
            VeeamTransportMessage response = psAgent.runCommand(vbrHost, command);

            if (response.status.Equals("Error"))
            {
                return BadRequest((String)response.message);
            }

            Collection<PSObject> buRepo = (Collection<PSObject>)response.message;

            if (buRepo[0] == null)
            {
                return NotFound();
            }

            VeeamBackupResource buResource = null;
            if (Boolean.Parse(buRepo[0].Properties["WanAccelerationEnabled"].Value.ToString()))
            {
                buResource = new VeeamBackupResource
                {
                    uid = buRepo[0].Properties["Id"].Value.ToString(),
                    backupRepositoryName = buRepo[1].ToString(),
                    repositoryDisplayName = buRepo[0].Properties["RepositoryFriendlyName"].Value.ToString(),
                    repositoryQuota = Int32.Parse(buRepo[0].Properties["RepositoryQuota"].Value.ToString()),
                    repositoryPath = buRepo[0].Properties["RepositoryQuotaPath"].Value.ToString(),
                    usedSpace = Int32.Parse(buRepo[0].Properties["UsedSpace"].Value.ToString()),
                    usedPercentage = Double.Parse(buRepo[0].Properties["UsedSpacePercentage"].Value.ToString()),
                    enableWanAccelerator = true,
                    wanAcceleratorName = buRepo[0].Properties["WanAccelerator"].Value.ToString()
                };
            }
            else
            {
                buResource = new VeeamBackupResource
                {
                    uid = buRepo[0].Properties["Id"].Value.ToString(),
                    backupRepositoryName = buRepo[1].ToString(),
                    repositoryDisplayName = buRepo[0].Properties["RepositoryFriendlyName"].Value.ToString(),
                    repositoryQuota = Int32.Parse(buRepo[0].Properties["RepositoryQuota"].Value.ToString()),
                    repositoryPath = buRepo[0].Properties["RepositoryQuotaPath"].Value.ToString(),
                    usedSpace = Int32.Parse(buRepo[0].Properties["UsedSpace"].Value.ToString()),
                    usedPercentage = Double.Parse(buRepo[0].Properties["UsedSpacePercentage"].Value.ToString()),
                };
            }

            return Ok(buResource);
        }

        [Route("{resourceUid}")]
        [HttpPut]
        [PSVeeamRESTAPI.Filters.Authorization]
        public IHttpActionResult EditBackupResource(string vbrHost, string tenantUid, string resourceUid, [FromBody] dynamic resource)
        {
            logger.Info("Received request to edit Veeam Backup resource: " + resourceUid + " for tenant: " + tenantUid);

            dynamic validate = schemaValidator.verifyJSONPayload("EditBackupResource", resource);

            // Check if VeeamTransportMessage type is returned. If so, schema filter found an 
            // error with the JSON payload so return the error message back. 
            if (validate is VeeamTransportMessage)
            {
                return BadRequest((String)validate.message);
            }

            String command = "";
            if (resource.enableWanAccelerator == true)
            {
                command = "$wanAccel = Get-VBRWANAccelerator -Name " + resource.wanAcceleratorName + "; $tenant = Get-VBRCloudTenant -Id '" + tenantUid + "'; " +
                "$buResource = $tenant | Select -ExpandProperty Resources | Where-Object {$_.Id -eq '" +
                resourceUid + "'}; $buResources = [System.Collections.ArrayList](@($tenant | Select -ExpandProperty Resources)); " +
                "$buResources.Remove($buResource); $newResource = Set-VBRCloudTenantResource -CloudTenantResource $buResource " +
                "-RepositoryFriendlyName '" + resource.repositoryDisplayName + "' -Repository $buResource.Repository " +
                "-Quota " + resource.repositoryQuota + " -EnableWanAccelerator -WanAccelerator $wanAccel; $buResources.Add($newResource); " +
                " Set-VBRCloudTenant -CloudTenant $tenant -Resources $buResources;";
            }
            else
            {
                command = "$tenant = Get-VBRCloudTenant -Id '" + tenantUid + "'; " +
                "$buResource = $tenant | Select -ExpandProperty Resources | Where-Object {$_.Id -eq '" +
                resourceUid + "'}; $buResources = [System.Collections.ArrayList](@($tenant | Select -ExpandProperty Resources)); " +
                "$buResources.Remove($buResource); $newResource = Set-VBRCloudTenantResource -CloudTenantResource $buResource " +
                "-RepositoryFriendlyName '" + resource.repositoryDisplayName + "' -Repository $buResource.Repository " +
                "-Quota " + resource.repositoryQuota + "; $buResources.Add($newResource); " +
                " Set-VBRCloudTenant -CloudTenant $tenant -Resources $buResources;";
            }

            VeeamTransportMessage response = psAgent.runCommand(vbrHost, command);

            if (response.status.Equals("Error"))
            {
                return BadRequest((String)response.message);
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        [Route("")]
        [HttpPost]
        [PSVeeamRESTAPI.Filters.Authorization]
        public IHttpActionResult NewBackupResource(string vbrHost, string tenantUid, [FromBody] dynamic resource)
        {
            logger.Info("Received request to add backup resource for tenant: " + tenantUid);
            dynamic validate = schemaValidator.verifyJSONPayload("NewBackupResource", resource);

            // Check if VeeamTransportMessage type is returned. If so, schema filter found an 
            // error with the JSON payload so return the error message back. 
            if (validate is VeeamTransportMessage)
            {
                return BadRequest((String)validate.message);
            }

            String command = ("$tenant = Get-VBRCloudTenant -Id '" + tenantUid + "'; " +
                "$repo = Get-VBRBackupRepository -Name '" + resource.backupRepositoryName + "'; " +
                "$buResources = New-Object System.Collections.ArrayList;" +
                "$currentResources = $tenant | Select -ExpandProperty Resources; " +
                "foreach($currentResource in $currentResources) {[void]$buResources.Add($currentResource)}; " +
                "$cloudRepo = New-VBRCloudTenantResource -Repository $repo -RepositoryFriendlyName '" +
                    resource.repositoryDisplayName + "' -Quota " + resource.repositoryQuota + "; " +
                "[void]$buResources.Add($cloudRepo); " + 
                "$tenantNewResource = Set-VBRCloudTenant -CloudTenant $tenant -Resources $buResources;" +
                "Get-VBRCloudTenant -Id '" + tenantUid + "' | Select -ExpandProperty Resources | " + 
                    "Where-Object {$_.RepositoryFriendlyName -eq '" + resource.repositoryDisplayName + "'};" );
            VeeamTransportMessage response = psAgent.runCommand(vbrHost, command);

            if (response.status.Equals("Error"))
            {
                return BadRequest((String)response.message);
            }

            Collection<PSObject> buRepo = (Collection<PSObject>)response.message;

            VeeamBackupResource buResource = null;
            if (Boolean.Parse(buRepo[0].Properties["WanAccelerationEnabled"].Value.ToString()))
            {
                buResource = new VeeamBackupResource
                {
                    uid = buRepo[0].Properties["Id"].Value.ToString(),
                    backupRepositoryName = resource.backupRepositoryName,
                    repositoryDisplayName = buRepo[0].Properties["RepositoryFriendlyName"].Value.ToString(),
                    repositoryQuota = Int32.Parse(buRepo[0].Properties["RepositoryQuota"].Value.ToString()),
                    repositoryPath = buRepo[0].Properties["RepositoryQuotaPath"].Value.ToString(),
                    usedSpace = Int32.Parse(buRepo[0].Properties["UsedSpace"].Value.ToString()),
                    usedPercentage = Double.Parse(buRepo[0].Properties["UsedSpacePercentage"].Value.ToString()),
                    wanAcceleratorName = buRepo[0].Properties["WanAccelerator"].Value.ToString(),
                };
            } else
            {
                buResource = new VeeamBackupResource
                {
                    uid = buRepo[0].Properties["Id"].Value.ToString(),
                    backupRepositoryName = resource.backupRepositoryName,
                    repositoryDisplayName = buRepo[0].Properties["RepositoryFriendlyName"].Value.ToString(),
                    repositoryQuota = Int32.Parse(buRepo[0].Properties["RepositoryQuota"].Value.ToString()),
                    repositoryPath = buRepo[0].Properties["RepositoryQuotaPath"].Value.ToString(),
                    usedSpace = Int32.Parse(buRepo[0].Properties["UsedSpace"].Value.ToString()),
                    usedPercentage = Double.Parse(buRepo[0].Properties["UsedSpacePercentage"].Value.ToString())
                };
            }

            // Build Code 201 Response Message
            HttpResponseMessage returnMessage = Request.CreateResponse<VeeamBackupResource>(HttpStatusCode.Created, buResource);

            return ResponseMessage(returnMessage);
        }
    }
}
