using Microsoft.AspNetCore.SignalR;
using PlcApiBackend.PlcComm;
using System.Threading.Tasks;
using PlcApiBackend.PlcComm; // Make sure this matches your PlcCommunicator namespace

namespace PlcApiBackend.Hubs // <-- IMPORTANT: Change PlcApiBackend to your project name
{
    public class PlcHub : Hub
    {
        private readonly PlcCommunicator _plcCommunicator;

        public PlcHub(PlcCommunicator plcCommunicator)
        {
            _plcCommunicator = plcCommunicator;
        }

        // This method can be called from the JavaScript client to write to PLC
        public async Task WriteD100Live(int value)
        {
            try
            {
                await _plcCommunicator.WriteDWordAsync("D100", value);
                // Push update to all clients after successful write (or the calling client specifically)
                // The background service also pushes, so this might be redundant if the PLC
                // confirms the write immediately, but it's good for direct feedback.
                await Clients.All.SendAsync("ReceivePlcUpdate", "D100", value);
            }
            catch (Exception ex)
            {
                // Send error back to the caller
                await Clients.Caller.SendAsync("ReceiveError", $"Failed to write D100: {ex.Message}");
                Console.WriteLine($"Error in PlcHub.WriteD100Live: {ex.Message}");
            }
        }
        // You could also add a method here to request a specific tag's current value on demand
        // public async Task RequestD100Value() { ... }
    }
}