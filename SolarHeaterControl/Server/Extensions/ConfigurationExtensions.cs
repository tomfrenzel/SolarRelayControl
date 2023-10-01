using System.Text.Json;
using SolarHeaterControl.Shared.Models;

namespace SolarHeaterControl.Server.Extensions
{
    public static class ConfigurationExtensions
    {
        public static void Set(this IConfiguration configuration, Settings settings)
        {
            var configJson = File.ReadAllText("appsettings.json");
            var config = JsonSerializer.Deserialize<Dictionary<string, object>>(configJson);

            foreach (var prop in typeof(Settings).GetProperties())
            {
                config[prop.Name] = prop.GetValue(settings);
            }

            var updatedConfigJson = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText("appsettings.json", updatedConfigJson);
        }
    }
}
