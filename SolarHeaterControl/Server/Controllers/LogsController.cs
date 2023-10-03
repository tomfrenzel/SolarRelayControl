using Microsoft.AspNetCore.Mvc;
using SolarHeaterControl.Server.Stores;
using SolarHeaterControl.Shared.Models;

namespace SolarHeaterControl.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LogsController : ControllerBase
    {
        private readonly LogStore logStore;

        public LogsController(LogStore logStore)
        {
            this.logStore = logStore;
        }

        [HttpGet]
        public IEnumerable<LogEntry> Get()
        {
            return logStore.GetLogs();
        }
    }
}