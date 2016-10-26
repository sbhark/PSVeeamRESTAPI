using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using PSVeeamRESTAPI.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace PSVeeamRESTAPI.Filters
{
    public class JSONSchemaValidator
    {

        // Validate incoming JSON payload with appropriate JSON Schema.
        public dynamic verifyJSONPayload(String jsonSchemaFileName, dynamic payload)
        {
            VeeamTransportMessage returnMessage = new VeeamTransportMessage();

            // Get JSON from Schema file 
            String jsonSchemaFile = HttpContext.Current.Server.MapPath("~/Schemas/" + jsonSchemaFileName +".json");
            String jsonSchema = System.IO.File.ReadAllText(jsonSchemaFile);

            JSchema schema = JSchema.Parse(jsonSchema);
            JObject payloadObject = JObject.Parse(payload.ToString());

            try
            {
                IList<ValidationError> errors;
                bool isValidPayload = payloadObject.IsValid(schema, out errors);

                if (payloadObject.IsValid(schema))
                {
                    return payload;
                } else
                {

                    var sb = new StringBuilder();
                    foreach (ValidationError error in errors) {
                        sb.AppendLine("Element: " + error.Path + ". Message: " + error.Message);
                    }

                    returnMessage.status = "False";
                    returnMessage.message = sb.ToString();
                    
                    return returnMessage;
                }
            } catch (Exception e)
            {
                returnMessage.status = "False";
                returnMessage.message = "Unknown error trying to validate payload. Exception: " + e.ToString();
                return returnMessage;
            }
        }
    }
}