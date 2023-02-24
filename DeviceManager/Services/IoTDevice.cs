using DeviceManager.Data;
using DeviceManager.Tools;
using Microsoft.Azure.Devices.Client;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace DeviceManager.Services
{
    internal class IoTDevice
    {

        private Thread? thread;
        private bool threadActive;
        private readonly DeviceClient deviceClient;
        private readonly CancellationToken cancellationToken;
        private readonly int id;
        private readonly ConcurrentQueue<IoTData> queueOfDataToSend;

        public IoTDevice(int id, string connectionString, CancellationToken cancellationToken)
        {
            this.cancellationToken = cancellationToken;
            this.id = id; 
            this.deviceClient = DeviceClient.CreateFromConnectionString(connectionString, TransportType.Mqtt);
            this.queueOfDataToSend = new ConcurrentQueue<IoTData>();
        }

        public async Task Start()
        {
            deviceClient.SetConnectionStatusChangesHandler((status, statusChangeReason) =>
            {
                Console.WriteLine($"Device {id}: status={status} statusChangeReason={statusChangeReason}");
            });

            await deviceClient.OpenAsync().ConfigureAwait(false);
            if (thread == null)
            {
                threadActive = true;
                thread = new Thread(new ThreadStart(RunThreadIoTDevice));
                thread.IsBackground = true;
                thread.Name = $"Thread to manage IoT Device {this.id}";
                thread.Start();
            }
        }

        public async Task Stop()
        {
            await deviceClient.CloseAsync();
            threadActive = false;
            if (thread != null)
                thread.Join(5000);
        }

        public void AddMessage(IoTData data)
        {
            queueOfDataToSend.Enqueue(data);
        }

        private void RunThreadIoTDevice()
        {
            try
            {
                while ((threadActive) && (!cancellationToken.IsCancellationRequested))
                {
                    if (queueOfDataToSend.TryDequeue(out IoTData? data))
                    {
                        SendIoTMessage(data).Wait();
                    }
                    Thread.Sleep(1000);
                }
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine($"IoTDevice {this.id} - User canceled");
            }
        }

        private async Task SendIoTMessage(IoTData data)
        {
            string messageBody = JsonSerializer.Serialize(data);
            using var message = new Message(Encoding.ASCII.GetBytes(messageBody))
            {
                ContentType = "application/json",
                ContentEncoding = "utf-8",
            };

            Console.WriteLine($"Device {id}: sending message - {messageBody}");
            await deviceClient.SendEventAsync(message);
        }
    }
}
