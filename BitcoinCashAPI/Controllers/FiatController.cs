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
        public decimal GetValue([FromQuery] string currency) => _fiatService.GetValue(currency);
    }
}