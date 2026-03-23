# Agent Guide

## Project Purpose

This project exposes a Raspberry Pi CPU temperature over Bluetooth Low Energy as a standard Health Thermometer service.

## Current Architecture

- `Program.cs`
  - Defines the BLE service UUID `0x1809` and characteristic UUID `0x2A6E`.
  - Creates a `CpuTemperature` reader from `Iot.Device.CpuTemperature`.
  - Connects to BlueZ through `DotnetBleServer`.
  - Starts BLE advertising with local name `PiTemperature`.
  - Registers one primary GATT service with one read-only characteristic.
  - Prints temperature readings to the console once per second until a key is pressed.

- `TemperatureCharacteristicSource.cs`
  - Implements `ICharacteristicSource`.
  - On `ReadValueAsync`, reads available CPU temperatures and returns the first valid value.
  - Encodes Celsius as BLE `sint16` with `0.01` degree resolution.
  - Returns an empty byte array if no valid reading is available.
  - Ignores writes via `WriteValueAsync`.

## Behavior Notes

- The service is read-only from the BLE client perspective.
- The exposed GATT characteristic uses the standard Temperature Celsius characteristic format.
- The characteristic currently returns the first valid sensor value reported by `CpuTemperature.ReadTemperatures()`.
- If temperature data is unavailable, the BLE read returns no payload instead of an error code or sentinel value.
- The console loop is only for local visibility and does not drive BLE reads.

## Dependencies

- `.NET 10`
- `DotnetBleServer` `0.1.0-snapshot`
- `Iot.Device.Bindings` `2.2.0`
- `System.Device.Gpio` `2.2.0`
- A Linux environment with BlueZ available
- Hardware and OS support for `Iot.Device.CpuTemperature`

## Safe Change Areas

- Change BLE name, UUIDs, or characteristic flags in `Program.cs`.
- Adjust read semantics or fallback behavior in `TemperatureCharacteristicSource.cs`.
- Add logging, validation, or richer error handling in either file.

## Caution Areas

- Keep `0x2A6E` encoding compatible with BLE expectations: signed 16-bit integer, `0.01` degree Celsius units.
- Do not change UUIDs casually if interoperability with standard BLE thermometer clients matters.
- Returning `Array.Empty<byte>()` on unavailable temperature may not be accepted by all BLE clients; verify client behavior before relying on it.
- `BitConverter.GetBytes` is little-endian on Raspberry Pi Linux, which matches the expected BLE little-endian byte order here.

## Useful Commands

```bash
dotnet build
dotnet run
```

## Recommended Next Improvements

- Add explicit handling for the no-temperature case.
- Add a small encoding test for the `0x2A6E` value conversion.
- Consider extracting BLE setup into a separate type if the project grows beyond a single service.
