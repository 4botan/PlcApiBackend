using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging; // Add this using
using System;
using System.Threading;
using System.Threading.Tasks;
using PlcApiBackend.Hubs; // Your SignalR Hub
using PlcApiBackend.PlcComm; // Your PLC communicator

namespace PlcApiBackend.Services // <-- IMPORTANT: Change PlcApiBackend to your project name
{
    public class PlcDataMonitorService : BackgroundService
    {
        private readonly ILogger<PlcDataMonitorService> _logger;
        private readonly PlcCommunicator _plcCommunicator;
        private readonly IHubContext<PlcHub> _hubContext;
        private int _currentD100Value = -1; // To track changes

        public PlcDataMonitorService(
            ILogger<PlcDataMonitorService> logger,
            PlcCommunicator plcCommunicator,
            IHubContext<PlcHub> hubContext)
        {
            _logger = logger;
            _plcCommunicator = plcCommunicator;
            _hubContext = hubContext;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("PlcDataMonitorService is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    int newD100Value = await _plcCommunicator.ReadDWordAsync("D100");

                    if (newD100Value != _currentD100Value)
                    {
                        _currentD100Value = newD100Value;
                        _logger.LogInformation($"D100 changed to: {newD100Value}. Notifying clients.");
                        // Push update to all connected clients
                        await _hubContext.Clients.All.SendAsync("ReceivePlcUpdate", "D100", newD100Value);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error reading PLC in background service.");
                    // Optionally, try to reconnect PlcCommunicator here if connection lost
                }

                await Task.Delay(1000, stoppingToken); // Poll every 1 second
            }

            _logger.LogInformation("PlcDataMonitorService is stopping.");
        }
    }
}