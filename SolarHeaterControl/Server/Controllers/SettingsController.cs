using Microsoft.AspNetCore.Mvc;
using SolarHeaterControl.Client;
using SolarHeaterControl.Shared.Models;

namespace SolarHeaterControl.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SettingsController : ControllerBase
    {
        public SettingsController()
        {
        }

        [HttpGet]
        public async Task<Settings> Get()
        {
            return ControlService.Settings;
        }

        [HttpPost]
        public async Task Update(Settings settings)
        {
            ControlService.Settings = settings;
        }
    }
}
