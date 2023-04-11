using EmergencyRoadsideAssistance.Models;
using EmergencyRoadsideAssistance.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.ComponentModel;

namespace EmergencyRoadsideAssistance.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ReservationController : ControllerBase
    {
        private readonly ILogger<ReservationController> _logger;
        public readonly IRoadsideAssistanceService _roasideAssistanceService;
        public readonly IDBService _dbService;

        public ReservationController(ILogger<ReservationController> logger, IDBService dbService, IRoadsideAssistanceService roadsideAssistanceService)
        {
            _logger = logger;
            _roasideAssistanceService = roadsideAssistanceService;
            _dbService = dbService;
        }

        [HttpGet("GetAll")]
        public async Task<IEnumerable<Reservation>> GetAll()
        {
            return await _dbService.GetReservations(false);
        }


        [HttpGet("GetActive")]
        public async Task<IEnumerable<Reservation>> GetActive()
        {
            return await _dbService.GetReservations(true);
        }


        [HttpPost("Reserve")]
        public ActionResult Reserve([FromBody] ReserveRequest request)
        {
            var assistant = _roasideAssistanceService.ReserveAssistant(new Customer(request.CustomerId), request.Location);
            return Ok(assistant);
        }

        [HttpPut("Release")]
        public ActionResult Release([FromBody] ReleaseRequest request)
        {
            _roasideAssistanceService.ReleaseAssistant(new Customer(request.CustomerId), new Assistant(request.AssistantId));
            return Ok();
        }

    }
}