using Microsoft.AspNetCore.Mvc;
using SolarHeaterControl.Server.Stores;
using SolarHeaterControl.Shared.Models;

namespace SolarHeaterControl.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HistoryController : ControllerBase
    {
        public HistoryController(LogStore logStore)
        {
            _logStore = logStore;
        }

        private readonly LogStore _logStore;

        [HttpGet]
        public IEnumerable<LogEntry> Get()
        {
            return _logStore.GetLogs();
        }
    }
}