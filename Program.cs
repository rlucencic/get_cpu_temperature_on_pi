using System;
using System.Threading;
using Iot.Device.CpuTemperature;

using var cpuTemperature = new CpuTemperature();
Console.WriteLine("Press any key to quit");

while (!Console.KeyAvailable) {
	if (cpuTemperature.IsAvailable) {
		var temperature = cpuTemperature.ReadTemperatures();
		foreach (var entry in temperature) {
			if (!double.IsNaN(entry.Temperature.DegreesCelsius)) {
				Console.WriteLine($"Temperature from {entry.Sensor}: {entry.Temperature.DegreesCelsius} °C");
			}
			else
			{
				Console.WriteLine("Unable to read temperature.");
			}
		}
	}
	else 
	{
		Console.WriteLine("CPU temperature is not available");
	}
	Thread.Sleep(1000);
}
