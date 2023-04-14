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

        [HttpGet("GetReservationForCustomer")]
        public async Task<bool> GetReservationForCustomer(int CustomerId)
        {
            var reservations = await _dbService.GetReservations(true);
            return reservations.Any(x => x.CustomerId == CustomerId);
        }

        [HttpPost("Reserve")]
        public async Task<Assistant> Reserve([FromBody] ReserveRequest request)
        {
            var assistant = await _roasideAssistanceService.ReserveAssistant(new Customer(request.CustomerId), request.Location);
            return assistant;
        }

        [HttpPut("Release")]
        public async Task<ActionResult> Release([FromBody] ReleaseRequest request)
        {
            await _roasideAssistanceService.ReleaseAssistant(new Customer(request.CustomerId), new Assistant(request.AssistantId));
            return Ok();
        }

    }
}