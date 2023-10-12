# Solar Relay Control
Solar Relay Control is an ASP.NET App for automatically controlling a Relay based on data from a solar inverter

## Installation
The App uses Blazor Server as the Backend and Blazor WebAssembly as Frontend. The communication happens through a SignalR connection.

How you install this App is completely up to you and you can find different options in the [Microsoft Docs](https://learn.microsoft.com/en-us/aspnet/core/blazor/host-and-deploy/?view=aspnetcore-6.0). 

### Example: Self-contained Linux installation
1. Publish the App
```bash
dotnet publish -c Release -r linux-x64
```
2. Make the App executable
```bash
cd SolarRelayControl\Server\bin\Release\net6.0\linux-x64
chmod +x .\SolarRelayControl.Server
```
3. Run the App
```bash
.\SolarRelayControl.Server
```
By default, the Frontend is reachable at Port 5000

## Supported Devices
### Inverters
- Huawei Sun2000 series

### Relays
- Shelly Pro 1
- Shelly Pro 1PM

## Contributing
Pull requests are welcome. For major changes, please open an issue first
to discuss what you would like to change.

### How to add support of a new inverter or relay
Use the `ISolarService` and `IRelayService` Interfaces and add them to the DI inside the Program.cs file

## License
[MIT](https://choosealicense.com/licenses/mit/)