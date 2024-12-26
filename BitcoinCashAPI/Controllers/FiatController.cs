using BitcoinCash.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BitcoinCash.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FiatController(IFiatService fiatService) : ControllerBase
    {        
        private readonly IFiatService _fiatService = fiatService;

        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> GetValue([FromQuery] string currency)
        {
            var value = await _fiatService.GetValue(currency);

            return value > 0 ? Ok(value) : StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}