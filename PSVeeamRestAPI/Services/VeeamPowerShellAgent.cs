using System;
using System.Management.Automation;
using System.Management.Automation.Runspaces;

using System.Security;
using System.Collections.ObjectModel;
using System.Text;
using PSVeeamRESTAPI.App_Start;
using PSVeeamRESTAPI.AppConfigurations;
using NLog;


namespace PSVeeamRESTAPI.Services
{
    public class VeeamPowerShellAgent
    {
        private static Configs configs = new Configs();
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static Configs config = new Configs();

        public VeeamPowerShellAgent(){  }

        public VeeamTransportMessage runCommand(string vbrHostIp, string command)
        {
            // Get Connection information of specified VBR
            VeeamPSProxiesCollection proxys = configs.veeamPsProxies;
            string vbrHost = "";
            string vbrUsername = "";
            string vbrPassword = "";

            int notFoundCount = 0;
            for (int i = 0; i < proxys.Count; i ++)
            {
                var proxy = proxys[i];
                if (proxy.hostNameOrIp.Equals(vbrHostIp))
                {
                    vbrHost = (String)proxy.hostNameOrIp;
                    vbrUsername = (String)proxy.username;
                    vbrPassword = (String)proxy.password;
                    break;
                }
                notFoundCount++;
            }
            if (notFoundCount == proxys.Count)
            {
                return new VeeamTransportMessage
                {
                    status = "Error",
                    message = "Unknown Veeam VBR Server HostName or IP"
                };
            }

            PowerShell psInstance = PowerShell.Create();
            var pass = new SecureString();
            Array.ForEach(vbrPassword.ToCharArray(), pass.AppendChar);
            var vbrCreds = new PSCredential(vbrUsername, pass);

            psInstance.AddCommand("Set-Variable");
            psInstance.AddParameter("Name", "cred");
            psInstance.AddParameter("Value", vbrCreds);

            // Run commands in their own runspaces so each request is unique and isloated.
            Runspace rs = RunspaceFactory.CreateRunspace();
            rs.Open();
            psInstance.Runspace = rs;

            VeeamTransportMessage response = new VeeamTransportMessage();

            logger.Info("Invoking powershell command to remote Veeam VBR");
            var remoteCommand = "Invoke-Command -ComputerName " + vbrHost + " -ScriptBlock {Add-PSSnapin VeeamPSSnapin; " + command + "} -credential $cred";
            psInstance.AddScript(remoteCommand);
            Collection<PSObject> taskOutput = psInstance.Invoke();

            // If errors present construct error message and return it
            if (psInstance.Streams.Error.Count > 0)
            {
                var sb = new StringBuilder();
                foreach(var error in psInstance.Streams.Error)
                {
                    sb.Append(error.ToString());
                }
                psInstance.Streams.Error.Clear();

                // Construct response message
                String message = sb.ToString();
                response.status = "Error";
                response.message = message;

                return response;
            }

            response.status = "Success";
            response.message = taskOutput;

            rs.Close();

            return response;
        }
    }
}