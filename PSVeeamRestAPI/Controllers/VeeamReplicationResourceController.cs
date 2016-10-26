using PSVeeamRESTAPI.Filters;
using PSVeeamRESTAPI.Models;
using PSVeeamRESTAPI.Services;
using System;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Net;
using System.Web.Http;

namespace PSVeeamRESTAPI.Controllers
{
    public class VeeamReplicationResourceController : ApiController
    {
        private static JSONSchemaValidator schemaValidator = new JSONSchemaValidator();
        private static VeeamPowerShellAgent psAgent = new VeeamPowerShellAgent();

        [HttpGet]
        [PSVeeamRESTAPI.Filters.Authorization]
        public IHttpActionResult GetReplicationResources(string vbrHost, string tenantUid)
        {
            String command = ("$hwResourceOption = Get-VBRCloudTenant -Id '" + tenantUid + "' | " + 
                "Select -ExpandProperty ReplicationResources | " + "Select -ExpandProperty HardwarePlanOptions; $hwResourceOption; " + 
                "$hwResource = Get-VBRCloudHardwarePlan -Id $hwResourceOption.HardwarePlanId; $hwResource; " + 
                "$datastore = $hwResourceOption | Select -ExpandProperty DatastoreQuota; $datastore");
            VeeamTransportMessage response = psAgent.runCommand(vbrHost, command);

            if (response.status.Equals("Error"))
            {
                return BadRequest((String)response.message);
            }

            Collection<PSObject> replicationResponse = (Collection<PSObject>)response.message;

            // Construct Replication Resource Object
            VeeamReplicationResource replicationResource = new VeeamReplicationResource();
            replicationResource.hwPlanOptionUid = replicationResponse[0].Properties["Id"].Value.ToString();
            replicationResource.hwPlanUid = replicationResponse[1].Properties["Id"].Value.ToString();
            replicationResource.hwPlanName = replicationResponse[1].Properties["Name"].Value.ToString();
            replicationResource.platform = replicationResponse[1].Properties["Platform"].Value.ToString();
            replicationResource.cpuQuota = Int32.Parse(replicationResponse[1].Properties["CPU"].Value.ToString());
            replicationResource.cpuUsed = Int32.Parse(replicationResponse[0].Properties["UsedCPU"].Value.ToString());
            replicationResource.memoryQuota = Int32.Parse(replicationResponse[1].Properties["Memory"].Value.ToString());
            replicationResource.memoryUsed = Int32.Parse(replicationResponse[0].Properties["UsedMemory"].Value.ToString());
            replicationResource.netWithInternet = Int32.Parse(replicationResponse[1].Properties["NumberOfNetWithInternet"].Value.ToString());
            replicationResource.netWithoutInternet = Int32.Parse(replicationResponse[1].Properties["NumberOfNetWithoutInternet"].Value.ToString());
            replicationResource.datastoreName = replicationResponse[2].Properties["DatastoreId"].Value.ToString();
            replicationResource.datastoreDisplayName = replicationResponse[2].Properties["FriendlyName"].Value.ToString();
            replicationResource.datastoreQuota = Int32.Parse(replicationResponse[2].Properties["Quota"].Value.ToString());
            replicationResource.datastoreUsed = Int32.Parse(replicationResponse[2].Properties["UsedSpace"].Value.ToString());

            if ((Boolean)replicationResponse[0].Properties["WanAccelerationEnabled"].Value)
            {
                replicationResource.wanAcceleratorId = replicationResponse[0].Properties["WanAccelerator"].Value.ToString();
            } 

            return Ok(replicationResource);
        }

        [HttpGet]
        [PSVeeamRESTAPI.Filters.Authorization]
        public IHttpActionResult GetReplicationResource(string vbrHost, string tenantUid, string resourceUid)
        {
            String command = ("$hwResourceOption = Get-VBRCloudTenant -Id '" + tenantUid + "' | " +
                "Select -ExpandProperty ReplicationResources | " + "Select -ExpandProperty HardwarePlanOptions | " + 
                "Where-Object {$_.Id -eq'" +resourceUid+ "'}; $hwResourceOption; " +
                "$hwResource = Get-VBRCloudHardwarePlan -Id $hwResourceOption.HardwarePlanId; $hwResource; " +
                "$datastore = $hwResourceOption | Select -ExpandProperty DatastoreQuota; $datastore");
            VeeamTransportMessage response = psAgent.runCommand(vbrHost, command);

            if (response.status.Equals("Error"))
            {
                return BadRequest((String)response.message);
            }

            Collection<PSObject> replicationResponse = (Collection<PSObject>)response.message;

            // Construct Replication Resource Object
            VeeamReplicationResource replicationResource = new VeeamReplicationResource();
            replicationResource.hwPlanOptionUid = replicationResponse[0].Properties["Id"].Value.ToString();
            replicationResource.hwPlanUid = replicationResponse[1].Properties["Id"].Value.ToString();
            replicationResource.hwPlanName = replicationResponse[1].Properties["Name"].Value.ToString();
            replicationResource.platform = replicationResponse[1].Properties["Platform"].Value.ToString();
            replicationResource.cpuQuota = Int32.Parse(replicationResponse[1].Properties["CPU"].Value.ToString());
            replicationResource.cpuUsed = Int32.Parse(replicationResponse[0].Properties["UsedCPU"].Value.ToString());
            replicationResource.memoryQuota = Int32.Parse(replicationResponse[1].Properties["Memory"].Value.ToString());
            replicationResource.memoryUsed = Int32.Parse(replicationResponse[0].Properties["UsedMemory"].Value.ToString());
            replicationResource.netWithInternet = Int32.Parse(replicationResponse[1].Properties["NumberOfNetWithInternet"].Value.ToString());
            replicationResource.netWithoutInternet = Int32.Parse(replicationResponse[1].Properties["NumberOfNetWithoutInternet"].Value.ToString());
            replicationResource.datastoreName = replicationResponse[2].Properties["DatastoreId"].Value.ToString();
            replicationResource.datastoreDisplayName = replicationResponse[2].Properties["FriendlyName"].Value.ToString();
            replicationResource.datastoreQuota = Int32.Parse(replicationResponse[2].Properties["Quota"].Value.ToString());
            replicationResource.datastoreUsed = Int32.Parse(replicationResponse[2].Properties["UsedSpace"].Value.ToString());

            if ((Boolean)replicationResponse[0].Properties["WanAccelerationEnabled"].Value)
            {
                replicationResource.wanAcceleratorId = replicationResponse[0].Properties["WanAccelerator"].Value.ToString();
            }

            return Ok(replicationResource);
        }

        [HttpPost]
        [PSVeeamRESTAPI.Filters.Authorization]
        public IHttpActionResult CreateReplicationResource(string vbrHost, string tenantUid, [FromBody] dynamic resource)
        {
            dynamic validate = schemaValidator.verifyJSONPayload("CreateReplicationResource", resource);

            // Check if VeeamTransportMessage type is returned. If so, schema filter found an 
            // error with the JSON payload so return the error message back. 
            if (validate is VeeamTransportMessage)
            {
                return BadRequest((String)validate.message);
            }

            String command = "";
            if (resource.platform == "VMWare")
            {
                command = ("$repCluster = (Find-VBRViEntity -Name 'V9-Replication')[1]; " + 
                    "$ds = Get-VBRServer -Name 10.10.1.41 | Find-VBRViDatastore -Name '" + resource.datastoreName + "'; " +
                    "$cloudDatastore = New-VBRViCloudHWPlanDatastore -Datastore $ds -FriendlyName '" + resource.datastoreDisplayName + 
                        "' -Quota " + resource.datastoreQuota + "; " +
                    "$cloudHwPlan = Add-VBRViCloudHardwarePlan -Name '" + resource.hwPlanName + "' -Description 'Created by PSVeeam PS RestAPI' " +
                        "-Server $repCluster -CPU " + resource.cpuQuota + " -Memory " + resource.memoryQuota + 
                        " -NumberOfNetWithInternet " + resource.netWithInternet + " -NumberOfNetWithoutInternet " + resource.netWithoutInternet +
                        " -Datastore $cloudDatastore; " + 
                    "$hwPlanOptions = New-VBRCloudTenantHwPlanOptions -HardwarePlan $cloudHwPlan; " + 
                    "$repResource = New-VBRCloudTenantReplicationResources -HardwarePlanOptions $hwPlanOptions -EnablePublicIp " + 
                        "-NumberOfPublicIp " + resource.publicIpCount +"; " +
                    "$tenant = Get-VBRCloudTenant -Id '" + tenantUid + "'; " +
                    "Set-VBRCloudTenant -CloudTenant $tenant -ReplicationResources $repResource;");
            } else
            {
                command = "";
            }
            VeeamTransportMessage response = psAgent.runCommand(vbrHost, command);

            if (response.status.Equals("Error"))
            {
                return BadRequest((String)response.message);
            }

            return StatusCode(HttpStatusCode.Created);
        }
    }
}
