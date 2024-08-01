using BitcoinCash.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BitcoinCash.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WalletController(IWalletService walletService) : ControllerBase
    {        
        private readonly IWalletService _walletService = walletService;

        [HttpGet]
        public IActionResult Get(string addresses, string currency)
        {
            var addressList = addresses.Split(',').ToList();

            var wallet = _walletService.GetWalletInfo(addressList, currency);

            return wallet != null ? Ok(wallet) : StatusCode(StatusCodes.Status500InternalServerError);
        }

        [HttpPost]
        [Route("GetBalances")]
        public IActionResult GetBalances([FromForm] string addresses)
        {
            var addressList = addresses.Split(',').ToList();

            var balances = _walletService.GetWalletBalances(addressList);

            return balances != null ? Ok(balances) : StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}