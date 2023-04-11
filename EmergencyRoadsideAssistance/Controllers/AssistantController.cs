using EmergencyRoadsideAssistance.Models;
using EmergencyRoadsideAssistance.Services;
using Microsoft.AspNetCore.Mvc;

namespace EmergencyRoadsideAssistance.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AssistantController : ControllerBase
    {
        private readonly ILogger<AssistantController> _logger;
        public readonly IRoadsideAssistanceService _roasideAssistanceService;
        public readonly IDBService _dbService;


        public AssistantController(ILogger<AssistantController> logger, IRoadsideAssistanceService roadsideAssistanceService, IDBService dBService)
        {
            _logger = logger;
            _roasideAssistanceService = roadsideAssistanceService;
            _dbService = dBService;
        }

        [HttpGet("GetNearestAssistant")]
        public async Task<IEnumerable<Assistant>> Get([FromQuery]Geolocation location)
        {
            return await _roasideAssistanceService.FindNearestAssistants(location, 5);
        }

        [HttpGet("GetAll")]
        public async Task<IEnumerable<Assistant>> GetAll()
        {
            return await _dbService.GetAssistants();
        }

        [HttpPut("UpdateAssistant")]
        public async Task<ActionResult> Update([FromBody]UpdateAssistantRequest request)
        {
            await _roasideAssistanceService.UpdateAssistantLocation(new Assistant(request.AssistantId), request.Location);
            return Ok();
        }
    }
}