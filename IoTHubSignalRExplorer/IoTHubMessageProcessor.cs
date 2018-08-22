using IoTHubTrigger = Microsoft.Azure.WebJobs.ServiceBus.EventHubTriggerAttribute;

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.EventHubs;
using System.Text;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace IoTHubChecker
{
    public static class IoTHubMessageProcessor
    {
        private static HttpClient client = new HttpClient();

        [FunctionName("IoTHubMessageProcessor")]
        public static async Task Run([IoTHubTrigger("messages/events",  Connection= "IoTHubReceiveEventsConnectionEndpoint")]EventData message, ILogger log)
        {
            log.LogInformation($"C# IoT Hub trigger function processed a message: {Encoding.UTF8.GetString(message.Body.Array)}");
            var signalR = new AzureSignalR(Environment.GetEnvironmentVariable("AzureSignalRConnectionString"));
            await signalR.SendAsync("hub1", "RefreshMessageForDevice", message.Properties["iothub-connection-device-id"], Encoding.UTF8.GetString(message.Body.Array));
        }
    }
}