using DeviceManager.Data;
using DeviceManager.Services;

// Set up a condition to quit the sample
Console.WriteLine("Press control-C to exit.");
using var cancellationTokenSource = new CancellationTokenSource();

Console.CancelKeyPress += (sender, eventArgs) =>
{
    Console.WriteLine("Exiting...");
    eventArgs.Cancel = true;
    cancellationTokenSource.Cancel();
};

await VSSummit2023Emulator.Instance().Emulate(cancellationTokenSource.Token);

Console.WriteLine("VSSummit 2023 Demo Finished.");
