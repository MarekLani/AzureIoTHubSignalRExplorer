using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using System;
using Microsoft.Azure.Devices;
using System.Threading.Tasks;
using System.Linq;
using System.Text;

namespace IoTHubChecker
{
    public static class RegisterDevice
    {
        [FunctionName("RegisterDevice")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]HttpRequest req, ILogger log)
        {
            log.LogInformation("Register Device Function Fired");
       
            string requestBody = new StreamReader(req.Body).ReadToEnd();
            dynamic jsonObject = JsonConvert.DeserializeObject(requestBody);

            RegistryManager rm = RegistryManager.CreateFromConnectionString(Environment.GetEnvironmentVariable("IoTHubConnectionString"));
            Device d = new Device((string)jsonObject.deviceId);

            var q = rm.CreateQuery($"SELECT * FROM devices WHERE deviceId = '{d.Id}'", 1);
            string token = string.Empty;

            try {

                var result = await q.GetNextAsJsonAsync();
                //Check if device already exists
                if (result.ToList().Any())
                {
                    //we call GetDeviceAsync asi IoT Hub query does not return symetric key
                    d = await rm.GetDeviceAsync(d.Id);
                    token = d.Authentication.SymmetricKey.PrimaryKey;
                }
                else
                {
                    //Otherwise we create and register new device
                    Device newdevice = new Device();

                    var primaryKey = Guid.NewGuid();
                    var secondaryKey = Guid.NewGuid();

                    byte[] bytes = Encoding.UTF8.GetBytes(primaryKey.ToString());
                    string base64PrimaryKey = Convert.ToBase64String(bytes);

                    bytes = Encoding.UTF8.GetBytes(secondaryKey.ToString());
                    string base64SecondaryKey = Convert.ToBase64String(bytes);

                    try
                    {

                        d.Authentication = new AuthenticationMechanism
                        {
                            SymmetricKey = new SymmetricKey
                            {
                                PrimaryKey = base64PrimaryKey,
                                SecondaryKey = base64SecondaryKey
                            }
                        };

                        newdevice = await rm.AddDeviceAsync(d);
                    }
                    catch (Exception e)
                    {
                        log.LogInformation("Exception caught creating new device: " + e.Message);
                        return (ActionResult)new BadRequestObjectResult("Exception caught creating new device: " + e.Message);
                    }
                    token = newdevice.Authentication.SymmetricKey.PrimaryKey;
                }
            }
            catch (Exception e)
            {
                log.LogInformation("Exception caught quering IoT Hub devices:" + e.Message);
                return (ActionResult)new BadRequestObjectResult("Exception caught quering IoT Hub devices:" + e.Message);

            }
       
            string connectionString = String.Format("HostName={0}.azure-devices.net;DeviceId={1};SharedAccessKey={2}", Environment.GetEnvironmentVariable("IoTHubHostName"), d.Id, token);
            return (ActionResult)new OkObjectResult(@"{""DeviceConnectionString:"":""" + connectionString + @"""}");
        }
    }
}
