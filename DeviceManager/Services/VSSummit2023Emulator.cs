using DeviceManager.Data;
using DeviceManager.Tools;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceManager.Services
{
    internal class VSSummit2023Emulator
    {
        private readonly ConcurrentDictionary<int, IoTDevice> _devices = new ConcurrentDictionary<int, IoTDevice>();
        private readonly ConcurrentDictionary<int, HardwareDevice> _HardwareDevices = new ConcurrentDictionary<int, HardwareDevice>();

        private static VSSummit2023Emulator? _object;

        public static VSSummit2023Emulator Instance()
        {
            if (_object == null)
                _object = new VSSummit2023Emulator();
            return _object;
        }

        internal async Task Emulate(CancellationToken cancellationToken)
        {
            AddIoTDevices(cancellationToken);
            AddHardwareDevices(cancellationToken);
            try
            {
                await StartDevices();
                while (!cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(1000, cancellationToken);
                }
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("User canceled");
            }
            finally
            {
                await CloseDevices();
            }
        }

        private void AddHardwareDevices(CancellationToken cancellationToken)
        {
            _HardwareDevices.TryAdd(1, new HardwareDevice(1, cancellationToken));
            _HardwareDevices.TryAdd(2, new HardwareDevice(2, cancellationToken));
            _HardwareDevices.TryAdd(3, new HardwareDevice(3, cancellationToken));
            _HardwareDevices.TryAdd(4, new HardwareDevice(4, cancellationToken));
            _HardwareDevices.TryAdd(5, new HardwareDevice(5, cancellationToken));
        }


        private async Task StartDevices()
        {
            foreach (var device in _devices.Values)
            {
                await device.Start();
            }
            foreach (var hardwareDevices in _HardwareDevices.Values)
            {
                hardwareDevices.OnNewMessage -= HardwareDevices_OnNewMessage;
                hardwareDevices.OnNewMessage += HardwareDevices_OnNewMessage;
                hardwareDevices.Start();
            }
        }

        private void HardwareDevices_OnNewMessage(object? sender, IoTData iotMessage)
        {
            if (sender is HardwareDevice device)
                _devices[device.DeviceId].AddMessage(iotMessage);
        }

        private async Task CloseDevices()
        {
            foreach (var device in _devices.Values)
            {
                await device.Stop();
            }
            foreach (var hardwareDevices in _HardwareDevices.Values)
            {
                hardwareDevices.Stop();
            }
        }

        private void AddIoTDevices(CancellationToken cancellationToken)
        {
            _devices.TryAdd(1, new IoTDevice(1, Constants.CS_DEVICE_01, cancellationToken));
            _devices.TryAdd(2, new IoTDevice(2, Constants.CS_DEVICE_02, cancellationToken));
            _devices.TryAdd(3, new IoTDevice(3, Constants.CS_DEVICE_03, cancellationToken));
            _devices.TryAdd(4, new IoTDevice(4, Constants.CS_DEVICE_04, cancellationToken));
            _devices.TryAdd(5, new IoTDevice(5, Constants.CS_DEVICE_05, cancellationToken));
        }
    }
}
