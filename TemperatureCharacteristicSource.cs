using DotnetBleServer.Gatt;
using Iot.Device.CpuTemperature;

internal class TemperatureCharacteristicSource : ICharacteristicSource
{
    private readonly CpuTemperature _cpuTemperature;

    internal TemperatureCharacteristicSource(CpuTemperature cpuTemperature)
    {
        _cpuTemperature = cpuTemperature;
    }

    public Task<byte[]> ReadValueAsync()
    {
        if (_cpuTemperature.IsAvailable)
        {
            foreach (var entry in _cpuTemperature.ReadTemperatures())
            {
                if (!double.IsNaN(entry.Temperature.DegreesCelsius))
                {
                    // BLE Temperature Celsius characteristic (0x2A6E):
                    // sint16, resolution 0.01 °C  (e.g. 36.5 °C → 3650)
                    short encoded = (short)(entry.Temperature.DegreesCelsius * 100);
                    return Task.FromResult(BitConverter.GetBytes(encoded));
                }
            }
        }

        return Task.FromResult(Array.Empty<byte>());
    }

    public Task WriteValueAsync(byte[] value) => Task.CompletedTask;
}
