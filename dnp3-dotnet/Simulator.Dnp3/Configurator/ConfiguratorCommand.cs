using System.Globalization;
using System.Text.Json;
using CsvHelper;
using Simulator.Dnp3.Common;
using Simulator.Dnp3.Configurator;

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Spectre.Console.Cli;

namespace Simulator.Dnp3.Configurator
{
    internal sealed class ConfiguratorCommand : AsyncCommand<ConfiguratorCommand.Settings>
    {
        public sealed class Settings : CommandSettings
        {
            [Description("Path to output. Defaults to current directory.")]
            [CommandArgument(0, "[searchPath]")]
            public string? OutputPath { get; init; }

            [CommandOption("-t|--type")]
            public string? DeviceType { get; init; }
        }

        public override async Task<int> ExecuteAsync([NotNull] CommandContext context, [NotNull] Settings settings)
        {
            var json = await File.ReadAllTextAsync("Outstations.json");
            var devicesConfigs = JsonSerializer.Deserialize<OutstationDeviceConfigs>(json) ?? new OutstationDeviceConfigs();

            var deviceType = settings.DeviceType;
            if(deviceType == null) throw new NullReferenceException("Missing Device Type");

            var points = new List<Dnp3Point>();
            foreach (var pointConfig in devicesConfigs[deviceType].Analog)
            {
                var dnp3Point = new Dnp3Point();
                dnp3Point.PointName = $"AnalogInput_index{pointConfig.Key}";
                dnp3Point.VolttronPointName = $"AnalogInput_index{pointConfig.Key}";
                dnp3Point.Index = int.Parse(pointConfig.Key);
                dnp3Point.Group = 30;
                dnp3Point.Variation = 6;
                dnp3Point.Scaling = 1;
                dnp3Point.Units = "NA";
                dnp3Point.Writable = false;
                dnp3Point.Notes = "Double Analogue input without status";
                points.Add(dnp3Point);
            }
            foreach (var pointConfig in devicesConfigs[deviceType].Binary)
            {
                var dnp3Point = new Dnp3Point();
                dnp3Point.PointName = $"BinaryInput_index{pointConfig.Key}";
                dnp3Point.VolttronPointName = $"BinaryInput_index{pointConfig.Key}";
                dnp3Point.Index = int.Parse(pointConfig.Key);
                dnp3Point.Group = 1;
                dnp3Point.Variation = 2;
                dnp3Point.Scaling = 1;
                dnp3Point.Units = "NA";
                dnp3Point.Writable = false;
                dnp3Point.Notes = "Single bit binary input with flags";
                points.Add(dnp3Point);
            }
            foreach (var pointConfig in devicesConfigs[deviceType].BinaryOutputStatus)
            {
                var dnp3Point = new Dnp3Point();
                dnp3Point.PointName = $"BinaryOutput_index{pointConfig.Key}";
                dnp3Point.VolttronPointName = $"BinaryOutput_index{pointConfig.Key}";
                dnp3Point.Index = int.Parse(pointConfig.Key);
                dnp3Point.Group = 10;
                dnp3Point.Variation = 2;
                dnp3Point.Scaling = 1;
                dnp3Point.Units = "NA";
                dnp3Point.Writable = true;
                dnp3Point.Notes = "Single bit binary Output with status";
                points.Add(dnp3Point);
            }
            var path = settings.OutputPath;
            if (path == null) throw new NullReferenceException("Missing Output Path");
            await using var writer = new StreamWriter(path);
            await using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
            csv.WriteRecords(points);
            writer.Flush();

            return 0;
        }
    }
}
