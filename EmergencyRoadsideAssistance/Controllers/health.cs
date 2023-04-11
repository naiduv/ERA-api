using Microsoft.AspNetCore.Mvc;

namespace EmergencyRoadsideAssistance.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HealthController : ControllerBase
    {

        [HttpGet(Name = "health")]
        public ActionResult Get()
        {
            return Ok("All good!");
        }
    }
}