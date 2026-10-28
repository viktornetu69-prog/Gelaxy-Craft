using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Backend.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

var app = builder.Build();
app.UseCors("AllowAll");
app.MapHub<GalaxyHub>("/galaxyhub");

Console.WriteLine("==========================================");
Console.WriteLine("  🚀 Galaxy Switch Backend Starting...");
Console.WriteLine("==========================================");
Console.WriteLine("  Server: http://localhost:5000");
Console.WriteLine("  Hub:    /galaxyhub");
Console.WriteLine("  Press Ctrl+C to stop.");
Console.WriteLine("==========================================");

app.Run();
