using System;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using dnp3;
using Simulator.Dnp3.Common;

namespace Simulator.Dnp3.Outstation
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            // Initialize logging with the default configuration
            Logging.Configure(
                new LoggingConfig(),
                new TestLogger()
            );
            
            var transport = Environment.GetEnvironmentVariable("TRANSPORT");
            var deviceType = Environment.GetEnvironmentVariable("DEVICE_TYPE");

            var json = await File.ReadAllTextAsync("Outstations.json");
            var devicesConfigs = JsonSerializer.Deserialize<OutstationDeviceConfigs>(json) ?? new OutstationDeviceConfigs();
            
            if ((string.IsNullOrWhiteSpace(transport) || string.IsNullOrWhiteSpace(deviceType)) && args.Length != 2)
            {
                System.Console.WriteLine("You must specify the device type");
                System.Console.WriteLine(
                    "Usage: outstation-example <transport> (tcp, serial, tls-ca, tls-self-signed) <type> (SEL651R2, SEL734P)");
                return;
            }
            
            deviceType ??= args[1];
            OutStationBase? device = deviceType switch
            {
                "SEL651R2" => new Sel651R2(devicesConfigs["SEL651R2"]),
                "SEL734P" => new Sel734P(devicesConfigs["SEL734P"]),
                _ => null
            };

            transport ??= args[0];
            device?.Run(transport);
        }
    }
}