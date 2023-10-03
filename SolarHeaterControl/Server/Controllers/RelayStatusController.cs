using Microsoft.AspNetCore.Mvc;
using SolarHeaterControl.Server.Services;
using SolarHeaterControl.Server.Stores;
using SolarHeaterControl.Shared.Models;

namespace SolarHeaterControl.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RelayStatusController : ControllerBase
    {
        private readonly RelayService relayService;

        public RelayStatusController(RelayService relayService)
        {
            this.relayService = relayService;
        }

        [HttpGet]
        public async Task<RelayStatusResponse> Get()
        {
            return await relayService.GetRelayStatus();
        }
    }
}