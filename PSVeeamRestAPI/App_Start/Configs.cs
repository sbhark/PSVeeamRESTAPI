using NLog;
using PSVeeamRESTAPI.AppConfigurations;
using System;
using System.Configuration;

namespace PSVeeamRESTAPI.App_Start
{
    public class Configs
    {
        private static readonly String apiAuthorization;
        private static readonly VeeamPSProxiesCollection psProxies;

        private static Logger logger = LogManager.GetCurrentClassLogger();

        static Configs()
        {
            string isDev = ConfigurationManager.AppSettings["isDev"];

            if (isDev.Equals("true"))
            {
                logger.Info("DEV Environment enabled.");

                logger.Info("Loading DEV Environment configurations");

                logger.Info("Reading DEV configurations");
                apiAuthorization = ConfigurationManager.AppSettings["devAuthorization"];

                // Veeam VBRs
                var psProxySection = ConfigurationManager.GetSection("devPsProxies");
                if (psProxySection != null)
                {
                    psProxies = (psProxySection as VeeamPSProxySection).proxys;
                } else
                {
                    logger.Error("Failed to find REQUIRED Veeam VBR Configurations");
                }


            } else
            {
                logger.Info("PROD Environment enabled, reading PROD configurations");

                logger.Info("Loading PROD Environment configurations");

                logger.Info("Configuration RAVEN for Sentry");

                logger.Info("Reading PROD configurations");
                apiAuthorization = ConfigurationManager.AppSettings["prodAuthorization"];

                // Veeam VBRs
                var psProxySection = ConfigurationManager.GetSection("prodPsProxies");
                if (psProxySection != null)
                {
                    psProxies = (psProxySection as VeeamPSProxySection).proxys;
                }
                else
                {
                    logger.Error("Failed to find REQUIRED Veeam VBR Configurations");
                }
            }
        }

        public string authorization
        {
            get
            {
                return apiAuthorization;
            }
        }

        public VeeamPSProxiesCollection veeamPsProxies
        {
            get
            {
                return psProxies;
            }
        }
    }
}