using Microsoft.AspNetCore.Mvc;
using SolarHeaterControl.Server.Extensions;
using SolarHeaterControl.Shared.Models;

namespace SolarHeaterControl.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SettingsController : ControllerBase
    {
        private readonly IConfiguration configuration;

        public SettingsController(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        [HttpGet]
        public async Task<Settings> Get()
        {
            return configuration.Get<Settings>();
        }

        [HttpPost]
        public async Task Update(Settings settings)
        {
            configuration.Set(settings);
        }
    }
}
