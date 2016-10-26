using Newtonsoft.Json;
using PSVeeamRESTAPI.App_Start;
using PSVeeamRESTAPI.Services;
using System;

using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace PSVeeamRESTAPI.Filters
{
    public class Authorization : AuthorizationFilterAttribute
    {

        private static String authorizationToken = null;
        private static Configs config = new Configs();

        // Constructor: Load Authorization Tokens when Object is constructed
        public Authorization()
        {
            authorizationToken = config.authorization;
        }

        public override void OnAuthorization(HttpActionContext actionContext)
        {
            var request = actionContext.Request;
            var header = request.Headers;

            if (header.Authorization != null)
            {
                String authorization = header.Authorization.ToString();
                if (!authorization.Equals(authorizationToken))
                {
                    throwUnauthorized(actionContext, "Invalid Authorization, please verify you have specified the correct Authorization token.");
                }
            } else
            {
                throwUnauthorized(actionContext, "Authorization header is required, please specify a valid Authorization token.");
            }
        }

        private void throwUnauthorized(HttpActionContext actionContext, String message)
        {
            HttpResponseMessage response = new HttpResponseMessage();
            VeeamTransportMessage responseObject = new VeeamTransportMessage();
            responseObject.status = "Failed";
            responseObject.message = message;
            var responseMessage = JsonConvert.SerializeObject(responseObject);
            response.Content = new StringContent(responseMessage, Encoding.UTF8, "application/json");
            response.StatusCode = HttpStatusCode.Forbidden;
            actionContext.Response = response;
        }
    }
}