using System;

using System.Net;
using System.Web.Http;
using System.Net.Http;
using System.Management.Automation;
using System.Collections.ObjectModel;

using PSVeeamRESTAPI.Models;
using PSVeeamRESTAPI.Services;
using PSVeeamRESTAPI.Filters;
using PSVeeamRESTAPI.App_Start;
using NLog;

namespace PSVeeamRESTAPI.Controllers
{
    [RoutePrefix("api/veeam/{vbrHost}/tenants")]
    public class VeeamTenantController : ApiController
    {
        private static JSONSchemaValidator schemaValidator = new JSONSchemaValidator();
        private static VeeamPowerShellAgent psAgent = new VeeamPowerShellAgent();
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static Configs config = new Configs();

        public VeeamTenantController() {}

        [Route("")]
        [HttpGet]
        [PSVeeamRESTAPI.Filters.Authorization]
        public IHttpActionResult GetAllTenants(string vbrHost)
        {
            logger.Info("Recevied request to get all Veeam tenants");

            String command = "Get-VBRCloudTenant";
            VeeamTransportMessage response = psAgent.runCommand(vbrHost, command);

            if (response.status.Equals("Error"))
            {

                logger.Error("Error retrieving all Veeam tenants");
                logger.Error((string)response.message);
            }

            Collection<VeeamTenant> veeamTenants = new Collection<VeeamTenant>();
            Collection<PSObject> tenants = (Collection<PSObject>)response.message;

            foreach (PSObject tenant in tenants)
            {
                VeeamTenant veeamTenant = new VeeamTenant();

                veeamTenant.uid = tenant.Properties["Id"].Value.ToString();
                veeamTenant.username = tenant.Properties["Name"].Value.ToString();
                veeamTenant.password = tenant.Properties["Password"].Value.ToString();
                if (tenant.Properties["LeaseExpirationDate"].Value != null)
                {
                    veeamTenant.leaseExpiration = tenant.Properties["LeaseExpirationDate"].Value.ToString();
                }

                veeamTenants.Add(veeamTenant);
            }

            return Ok(veeamTenants);
        }

        [Route("{tenantUid}")]
        [HttpGet]
        [PSVeeamRESTAPI.Filters.Authorization]
        public IHttpActionResult GetVeeamTenant(string vbrHost, string tenantUid)
        {
            logger.Info("Received request to retrieve veeam tenant");
            String command = "Get-VBRCloudTenant -Id '" + tenantUid + "'";
            VeeamTransportMessage response =  psAgent.runCommand(vbrHost, command);

            logger.Info("Checking response from veeam ");
            if (response.status.Equals("Error"))
            {
                logger.Info("Error retreiving veeam tenant");
                return BadRequest((String)response.message);
            }

            Collection<PSObject> taskOutput = (Collection<PSObject>)response.message;

            VeeamTenant tenant = new VeeamTenant();
            tenant.uid = taskOutput[0].Properties["Id"].Value.ToString();
            tenant.username = taskOutput[0].Properties["Name"].Value.ToString();
            tenant.password = taskOutput[0].Properties["Password"].Value.ToString();
            if (bool.Parse(taskOutput[0].Properties["LeaseExpirationEnabled"].Value.ToString()) == true)
            {
                tenant.leaseExpiration = taskOutput[0].Properties["LeaseExpirationDate"].Value.ToString();
            }

            return Ok(tenant);     
        }
        
        [Route("{tenantUid}")]
        [HttpDelete]
        [PSVeeamRESTAPI.Filters.Authorization]
        public IHttpActionResult DeleteVeeamTenant(string vbrHost, string tenantUid)
        {
            logger.Info("Received request to delete veeam tenant");
            String command = "Get-VBRCloudTenant -Id '" + tenantUid + "' | Remove-VBRCloudTenant";
            VeeamTransportMessage response = psAgent.runCommand(vbrHost, command);
            
            if (response.status.Equals("Error"))
            {
                return BadRequest((String) response.message);
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        [Route("{tenantUid}/disable")]
        [ActionName("Disable")]
        [HttpPut]
        [PSVeeamRESTAPI.Filters.Authorization]
        public IHttpActionResult DisableVeeamTenant(string vbrHost, string tenantUid)
        {
            logger.Info("Received request to disable veeam tenant");
            String command = "Get-VBRCloudTenant -Id '" + tenantUid + "' | Disable-VBRCloudTenant";
            VeeamTransportMessage response = psAgent.runCommand(vbrHost, command);

            if (response.status.Equals("Error"))
            {
                return BadRequest((String)response.message);
            }
            return StatusCode(HttpStatusCode.NoContent);
        }

        [Route("{tenantUid}/enable")]
        [ActionName("Enable")]
        [HttpPut]
        [PSVeeamRESTAPI.Filters.Authorization]
        public IHttpActionResult EnableVeeamTenant(string vbrHost, string tenantUid)
        {
            logger.Info("Recieved request to enable veeam tenant");
            String command = "Get-VBRCloudTenant -Id '" + tenantUid + "' | Enable-VBRCloudTenant";
            VeeamTransportMessage response = psAgent.runCommand(vbrHost, command);

            if (response.status.Equals("Error"))
            {
                return BadRequest((String)response.message);
            }
            return StatusCode(HttpStatusCode.NoContent);
        }

        [Route("{tenantUid}/password")]
        [HttpPut]
        [PSVeeamRESTAPI.Filters.Authorization]
        public IHttpActionResult EditTenantPassword(string vbrHost, string tenantUid, [FromBody] dynamic tenant)
        {

            logger.Info("Received request to edit tenant password");
            dynamic validate = schemaValidator.verifyJSONPayload("EditTenantPass", tenant);

            // Check if VeeamTransportMessage type is returned. If so, schema filter found an 
            // error with the JSON payload so return the error message back. 
            if (validate is VeeamTransportMessage)
            {
                return BadRequest((String)validate.message);
            }

            String command = "$tenant = Get-VBRCloudTenant -Id '" + tenantUid +
                "'; Set-VBRCloudTenant -CloudTenant $tenant -Password '" + tenant.password + "';";
            VeeamTransportMessage response = psAgent.runCommand(vbrHost, command);

            if (response.status.Equals("Error"))
            {
                return BadRequest((String)response.message);
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        [Route("{tenantUid}/lease")]
        [HttpPut]
        [PSVeeamRESTAPI.Filters.Authorization]
        public IHttpActionResult EditTenantLeaseDate(string vbrHost, string tenantUid, [FromBody] dynamic tenant)
        {
            logger.Info("Received request to edit tenant lease end date");
            dynamic validate = schemaValidator.verifyJSONPayload("EditTenantLeaseDate", tenant);

            // Check if VeeamTransportMessage type is returned. If so, schema filter found an 
            // error with the JSON payload so return the error message back. 
            if (validate is VeeamTransportMessage)
            {
                return BadRequest((String)validate.message);
            }

            String command = "$tenant = Get-VBRCloudTenant -Id '" + tenantUid +
            "'; Set-VBRCloudTenant -CloudTenant $tenant -EnableLeaseExpiration -LeaseExpirationDate '" + tenant.leaseExpiration + "'";

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
        public HttpResponseMessage PostVeeamTenant(string vbrHost, [FromBody]dynamic newTenant)
        {
            logger.Info("Received request to create new veeam tenant");
            dynamic validate = schemaValidator.verifyJSONPayload("NewTenant", newTenant);

            // Check if VeeamTransportMessage type is returned. If so, schema filter found an 
            // error with the JSON payload so return the error message back. 
            if (validate is VeeamTransportMessage)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, (String)validate.message);
            } 

            VeeamTenant tenant = null;
            String command = "";
            if (newTenant.leaseExpiration != null)
            {
                command = "Add-VBRCloudTenant -Name '" + newTenant.username + "' -Password '" + newTenant.password +
                    "' -Description 'Created from PSVeeamRestPowerShell' -EnableLeaseExpiration -LeaseExpirationDate '" + newTenant.leaseExpiration + "'";
            } else
            {
                command = "Add-VBRCloudTenant -Name '" + newTenant.username + "' -Password '" + newTenant.password +
                    "' -Description 'Created from PSVeeamRestPowerShell'";
            }
            VeeamTransportMessage response = psAgent.runCommand(vbrHost, command);

            if (response.status.Equals("Error"))
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, (String)response.message); 
            }

            Collection<PSObject> taskOutput = (Collection<PSObject>)response.message;

            tenant = new VeeamTenant();
            tenant.uid = taskOutput[0].Properties["Id"].Value.ToString();
            tenant.username = taskOutput[0].Properties["Name"].Value.ToString();
            tenant.password = taskOutput[0].Properties["Password"].Value.ToString();

            if (bool.Parse(taskOutput[0].Properties["LeaseExpirationEnabled"].Value.ToString()) == true)
            {
                tenant.leaseExpiration = taskOutput[0].Properties["LeaseExpirationDate"].Value.ToString();
            }

            return Request.CreateResponse(HttpStatusCode.Created, tenant);
        }

    }
}
