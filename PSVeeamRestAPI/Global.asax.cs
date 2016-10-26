using System.Web.Http;

using System;
using PSVeeamRESTAPI.App_Start;
using NLog;

namespace PSVeeamRESTAPI
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        protected void Application_Start()
        {
            logger.Info("API Startup, running initial configuration...");
            GlobalConfiguration.Configure(WebApiConfig.Register);

            logger.Info("Loading configurations from config file...");
            Configs configs = new Configs();

            logger.Info("API Startup complete.");
        }

        public void Application_Error(object sender, EventArgs e)
        {
            Exception excep = Server.GetLastError();
            logger.Info("API encountered unknown Application error, creatings logs...");
            logger.Info(excep);

            Server.ClearError();
        }
    }
}
