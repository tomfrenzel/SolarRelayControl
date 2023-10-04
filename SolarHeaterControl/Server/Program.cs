using Microsoft.AspNetCore.ResponseCompression;
using Serilog;
using SolarHeaterControl.Server.Hubs;
using SolarHeaterControl.Server.Services;
using SolarHeaterControl.Server.Services.Background;
using SolarHeaterControl.Server.Stores;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.SetBasePath(builder.Environment.ContentRootPath);
builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day, shared: true)
                .CreateLogger();

builder.Services.AddSignalR();
builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
       new[] { "application/octet-stream" });
});

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<LogStore>();
builder.Services.AddSingleton<CommunicationHub>();
builder.Services.AddSingleton<ModbusService>();
builder.Services.AddSingleton<RelayService>();
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
app.MapControllers();
app.MapHub<CommunicationHub>("/communicationhub");
app.MapFallbackToFile("index.html");

app.Run();
