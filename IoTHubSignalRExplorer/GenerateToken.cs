
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using System;

namespace IoTHubChecker
{
    public static class GenerateToken
    {
        [FunctionName("GenerateToken")]
        public static IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequest req, ILogger log)
        {
            log.LogInformation("Token Generation Requested");
            var signalR = new AzureSignalR(Environment.GetEnvironmentVariable("AzureSignalRConnectionString"));

            return new OkObjectResult(new
            {
                authInfo = new
                {
                    serviceUrl = signalR.GetClientHubUrl(Environment.GetEnvironmentVariable("HubName")),
                    accessToken = signalR.GenerateAccessToken(Environment.GetEnvironmentVariable("HubName"))
                },
            });
        }
    }
}
