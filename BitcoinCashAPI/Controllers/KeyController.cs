using BitcoinCash.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BitcoinCash.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class KeyController(IKeyService keyService) : ControllerBase
    {        
        private readonly IKeyService _keyService = keyService;

        [HttpGet]
        public IActionResult Get()
        {
            if (!_keyService.CanGetKey())
                return StatusCode(StatusCodes.Status429TooManyRequests);

            var key = _keyService.GetKey();

            return !string.IsNullOrWhiteSpace(key) ? Ok(key) : StatusCode(StatusCodes.Status500InternalServerError);
        }

        [HttpGet]
        [Route("RemainingCount")]
        public IActionResult GetRemainingRequestCount(string key)
        {
            var requestsCount = _keyService.GetCalls(key);

            if (requestsCount == null)
                return StatusCode(StatusCodes.Status404NotFound);

            return Ok(requestsCount);
        }
    }
}