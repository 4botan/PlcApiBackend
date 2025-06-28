using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PlcApiBackend.Hubs; // We'll create this soon
using PlcApiBackend.Services; // We'll create this soon
using PlcApiBackend.PlcComm; // We'll create this soon
// Add these using statements:
using Microsoft.Extensions.Logging; // For logging in the service
using System; // For Random in simulated PlcCommunicator

var builder = WebApplication.CreateBuilder(args);

// --- Services ---
builder.Services.AddControllers();
builder.Services.AddSignalR(); // Add SignalR services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure CORS for frontend access
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontendOrigin",
        policy => policy.WithOrigins(
                            "http://127.0.0.1:5500", // Common for VS Code Live Server
                            "http://localhost:8080",  // Another common dev server port
                            "https://localhost:7000", // Your backend's HTTPS URL (can be your own)
                            "http://localhost:5000"   // Your backend's HTTP URL (can be your own)
                        )
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials()); // Important for SignalR
});

// Register your PLC Communicator (adjust based on your chosen library)
builder.Services.AddSingleton<PlcCommunicator>(sp =>
{
    // **IMPORTANT: REPLACE WITH YOUR ACTUAL PLC IP AND PORT**
    // For simulation, leave as is, but for real PLC, change these:
    return new PlcCommunicator("192.168.1.10", 5000); // Example PLC IP and Port
});

// Register a background service to poll the PLC and send SignalR updates
builder.Services.AddHostedService<PlcDataMonitorService>();


var app = builder.Build();

// --- Middleware ---
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}
// app.UseHttpsRedirection(); // Keep this line, it's good practice

app.UseRouting(); // Must be before UseCors and MapHub/MapControllers

app.UseCors("AllowFrontendOrigin"); // Apply the CORS policy

app.UseAuthorization(); // If you implement authentication

app.MapControllers(); // Maps your API controllers
app.MapHub<PlcHub>("/plchub"); // Map your SignalR Hub to a URL path


app.Run();

//var builder = WebApplication.CreateBuilder(args);

//// Add services to the container.

//builder.Services.AddControllers();
//// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

//var app = builder.Build();

//// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

//app.UseHttpsRedirection();

//app.UseAuthorization();

//app.MapControllers();

//app.Run();
