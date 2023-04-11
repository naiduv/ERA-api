using EmergencyRoadsideAssistance.Models;
using Microsoft.AspNetCore.Mvc;

namespace EmergencyRoadsideAssistance.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CustomerController : ControllerBase
    {
        private readonly ILogger<CustomerController> _logger;

        public CustomerController(ILogger<CustomerController> logger)
        {
            _logger = logger;
        }

    }
}