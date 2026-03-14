using DotnetBleServer.Advertisements;
using DotnetBleServer.Core;
using DotnetBleServer.Gatt;
using DotnetBleServer.Gatt.Description;
using Iot.Device.CpuTemperature;

// Standard BLE Health Thermometer Service (0x1809)
const string serviceUuid = "00001809-0000-1000-8000-00805f9b34fb";
// Standard BLE Temperature Celsius Characteristic (0x2A6E) — sint16, 0.01 °C
const string temperatureCharUuid = "00002a6e-0000-1000-8000-00805f9b34fb";

using var cpuTemperature = new CpuTemperature();

Console.WriteLine("Starting BLE Temperature Server...");

using var serverContext = new ServerContext();
await serverContext.Connect();

// --- Advertisement ---
var advertisementProperties = new AdvertisementProperties
{
    Type = "peripheral",
    ServiceUUIDs = new[] { serviceUuid },
    LocalName = "PiTemperature",
};
await new AdvertisingManager(serverContext).CreateAdvertisement(advertisementProperties);

// --- GATT Application ---
var gattServiceDescription = new GattServiceDescription
{
    UUID = serviceUuid,
    Primary = true,
};

var gattCharacteristicDescription = new GattCharacteristicDescription
{
    UUID = temperatureCharUuid,
    Flags = new[] { "read" },
    CharacteristicSource = new TemperatureCharacteristicSource(cpuTemperature),
};

var gab = new GattApplicationBuilder();
gab.AddService(gattServiceDescription)
   .WithCharacteristic(gattCharacteristicDescription, Array.Empty<GattDescriptorDescription>());

await new GattApplicationManager(serverContext).RegisterGattApplication(gab.BuildServiceDescriptions());

Console.WriteLine("BLE server running.");
Console.WriteLine($"  Service UUID  : {serviceUuid}  (Health Thermometer 0x1809)");
Console.WriteLine($"  Characteristic: {temperatureCharUuid}  (Temperature °C 0x2A6E)");
Console.WriteLine("Connect with a BLE client and read the characteristic to get the CPU temperature.");
Console.WriteLine("Press any key to quit.");

while (!Console.KeyAvailable)
{
    if (cpuTemperature.IsAvailable)
    {
        var temperatures = cpuTemperature.ReadTemperatures();
        foreach (var entry in temperatures)
        {
            if (!double.IsNaN(entry.Temperature.DegreesCelsius))
                Console.WriteLine($"Temperature from {entry.Sensor}: {entry.Temperature.DegreesCelsius:F1} °C");
            else
                Console.WriteLine("Unable to read temperature.");
        }
    }
    else
    {
        Console.WriteLine("CPU temperature is not available.");
    }

    await Task.Delay(1000);
}
