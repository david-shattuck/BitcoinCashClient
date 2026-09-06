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
            if (!_keyService.CanGet())
                return StatusCode(StatusCodes.Status429TooManyRequests);

            var key = _keyService.Get();

            return Ok(new
            {
                key.Secret,
                key.Address,
                key.RemainingCalls
            });
        }

        [HttpGet]
        [Route("Info")]
        public IActionResult GetInfo(string secret)
        {
            if (!_keyService.CanGet())
                return StatusCode(StatusCodes.Status429TooManyRequests);

            var key = _keyService.Get(secret);

            if (key == null)
                return StatusCode(StatusCodes.Status404NotFound);

            return Ok(new
            {
                key.Secret,
                key.Address,
                key.RemainingCalls
            });
        }
    }
}