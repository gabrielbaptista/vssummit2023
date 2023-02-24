using DeviceManager.Data;
using DeviceManager.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DeviceManager.Services
{
    internal class HardwareDevice
    {
        private Thread? thread;
        private bool threadActive;
        private readonly CancellationToken cancellationToken;
        public int DeviceId { get; private set; }
        public event EventHandler<IoTData>? OnNewMessage;

        public HardwareDevice(int device, CancellationToken cancellationToken)
        {
            this.cancellationToken = cancellationToken;
            DeviceId = device;
        }


        internal void Start()
        {
            if (thread == null)
            {
                threadActive = true;
                thread = new Thread(new ThreadStart(RunThreadHardwareDevice));
                thread.IsBackground = true;
                thread.Name = $"Thread to manage Hardware Device {DeviceId}";
                thread.Start();
            }
        }

        internal void Stop()
        {
            threadActive = false;
            if (thread != null)
                thread.Join(5000);
        }

        private void RunThreadHardwareDevice()
        {
            try
            {
                while ((threadActive) && (!cancellationToken.IsCancellationRequested))
                {
                    var message = GenerateMessage();
                    OnNewMessage?.Invoke(this, message);
                    Thread.Sleep(1000);
                }
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine($"HardwareDevice {DeviceId} - User canceled");
            }
        }


        private IoTData GenerateMessage()
        {
            IoTData data = new IoTData();
            data.Status = "active";
            data.Value = DeviceId * 10;
            data.Date = DateTime.Now;
            data.SingleIndex = Utils.GetNextIndex();
            return data;
        }

    }
}
