using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR; // For IHubContext
using PlcApiBackend.Hubs;
using PlcApiBackend.PlcComm;
using System;
using System.Threading.Tasks;
using PlcApiBackend.Hubs; // Your SignalR Hub
using PlcApiBackend.PlcComm; // Your PLC communicator

namespace PlcApiBackend.Controllers // <-- IMPORTANT: Change PlcApiBackend to your project name
{
    [ApiController]
    [Route("api/[controller]")] // e.g., /api/plc
    public class PlcController : ControllerBase
    {
        private readonly PlcCommunicator _plcCommunicator;
        private readonly IHubContext<PlcHub> _hubContext; // To notify clients after REST write

        public PlcController(PlcCommunicator plcCommunicator, IHubContext<PlcHub> hubContext)
        {
            _plcCommunicator = plcCommunicator;
            _hubContext = hubContext;
        }

        [HttpGet("read/{address}")] // GET /api/plc/read/D100
        public async Task<IActionResult> ReadPlcAddress(string address)
        {
            try
            {
                // For simplicity, we'll only allow D100 for this example
                if (address.ToUpper() != "D100")
                {
                    return BadRequest(new { success = false, message = "Only D100 is supported for this example." });
                }
                int value = await _plcCommunicator.ReadDWordAsync(address);
                return Ok(new { success = true, address = address, value = value });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error reading {address}: {ex.Message}" });
            }
        }

        [HttpPost("write/{address}")] // POST /api/plc/write/D100 with JSON body { "value": 123 }
        public async Task<IActionResult> WritePlcAddress(string address, [FromBody] PlcWriteRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { success = false, message = "Invalid request body." });
            }
            // For simplicity, we'll only allow D100 for this example
            if (address.ToUpper() != "D100")
            {
                return BadRequest(new { success = false, message = "Only D100 is supported for this example." });
            }


            try
            {
                await _plcCommunicator.WriteDWordAsync(address, request.Value);
                // After writing, notify all connected clients via SignalR
                await _hubContext.Clients.All.SendAsync("ReceivePlcUpdate", address, request.Value);

                return Ok(new { success = true, message = $"Successfully wrote {request.Value} to {address}" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error writing to {address}: {ex.Message}" });
            }
        }
    }

    public class PlcWriteRequest
    {
        public int Value { get; set; }
    }
}