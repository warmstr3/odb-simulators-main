namespace Simulator.Dnp3.Common;

public class OutstationDeviceConfigs : Dictionary<string, OutstationDeviceConfig>
{
}

public class OutstationDeviceConfig
{
    public Dictionary<string, double> Analog { get; set; } = new Dictionary<string, double>();
    public Dictionary<string, string> Binary { get; set; } = new Dictionary<string, string>();
    public Dictionary<string, string> BinaryOutputStatus { get; set; } = new Dictionary<string, string>();
}