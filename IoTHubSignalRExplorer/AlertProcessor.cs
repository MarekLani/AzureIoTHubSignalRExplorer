using Microsoft.Azure.Devices;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace IoTHubChecker
{
    public static class AlertProcessor
    {
        [FunctionName("AlertProcessor")]
        public static async Task Run([EventHubTrigger("alerteh", Connection = "AlertEHEndpoint")]string myEventHubMessage, ILogger log)
        {
            try
            {
                var s_serviceClient = ServiceClient.CreateFromConnectionString(System.Environment.GetEnvironmentVariable(
                    "IoTHubConnectionString", EnvironmentVariableTarget.Process));

                dynamic jsonObject = JsonConvert.DeserializeObject(myEventHubMessage);

                var methodInvocation = new CloudToDeviceMethod("StartAlarm") { ResponseTimeout = TimeSpan.FromSeconds(30) };
                methodInvocation.SetPayloadJson(@"{""CO2"":""" + (string)jsonObject.avgco2 + @"""}");
                //methodInvocation.SetPayloadJson("10");
                // Invoke the direct method asynchronously and get the response from the simulated device.
                var response = await s_serviceClient.InvokeDeviceMethodAsync((string)jsonObject.deviceid, methodInvocation);

                Console.WriteLine("Response status: {0}, payload:", response.Status);
                Console.WriteLine(response.GetPayloadAsJson());
            }
            catch(Exception)
            {

            }
        }
    }
}
