using Microsoft.Azure.WebJobs;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IoTHubChecker
{
    public static class UpdateListOfDevices
    {
        [FunctionName("UpdateListOfDevices")]
        public static async Task Run([TimerTrigger("*/10 * * * * *", RunOnStartup =true)] TimerInfo timerTriggerInfo, ILogger log)
        {
            RegistryManager rm = RegistryManager.CreateFromConnectionString(Environment.GetEnvironmentVariable("IoTHubConnectionString"));
            var q = rm.CreateQuery("select * from devices", 10);
            var result = await q.GetNextAsJsonAsync();

            var devices = new List<DeviceInfo>();
            //var signalR = new AzureSignalR(Environment.GetEnvironmentVariable("AzureSignalRConnectionString"));
            var signalR = new AzureSignalR(Environment.GetEnvironmentVariable("AzureSignalRConnectionString"));

            foreach (var d in result.ToList())
            {
                devices.Add(new DeviceInfo() { Id = JsonConvert.DeserializeObject<Device>(d).Id });
            }
            await signalR.SendAsync("hub1", "RefreshDeviceList", JsonConvert.SerializeObject(devices));
                
        }
    }

    public class DeviceInfo
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }
}
