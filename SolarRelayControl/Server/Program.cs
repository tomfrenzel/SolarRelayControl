using Microsoft.AspNetCore.ResponseCompression;
using Serilog;
using SolarRelayControl.Server.Hubs;
using SolarRelayControl.Server.Interfaces;
using SolarRelayControl.Server.Services;
using SolarRelayControl.Server.Services.Relay;
using SolarRelayControl.Server.Services.Solar;
using SolarRelayControl.Server.Stores;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.SetBasePath(builder.Environment.ContentRootPath);
builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
builder.Host.UseSerilog((hostingContext, loggerConfiguration) =>
    loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration));

builder.Services.AddSignalR();
builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
       new[] { "application/octet-stream" });
});

builder.Services.AddRazorPages();
builder.Services.AddHttpClient();

// Inverter services
builder.Services.AddSingleton<ISolarService, Sun2000Service>();

// Relay services
builder.Services.AddSingleton<IRelayService, ShellyRelayService>();

// Base services
builder.Services.AddSingleton<LogStore>();
builder.Services.AddHostedService<ControlService>();
builder.Services.AddHostedService<RelayStatusService>();

var app = builder.Build();
app.UseResponseCompression();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();
app.UseRouting();

app.MapRazorPages();

app.MapHub<CommunicationHub>("/communicationhub");
app.MapHub<SettingsHub>("/settingshub");

app.MapFallbackToFile("index.html");

app.Run();