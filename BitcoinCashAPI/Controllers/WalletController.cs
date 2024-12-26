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
        public async Task<IActionResult> Get(string addresses, string currency)
        {
            var addressList = addresses.Split(',').ToList();

            var wallets = await _walletService.GetWalletInfo(addressList, currency);

            return wallets != null ? Ok(wallets) : StatusCode(StatusCodes.Status500InternalServerError);
        }

        [HttpPost]
        [Route("GetBalances")]
        public async Task<IActionResult> GetBalances([FromForm] string addresses)
        {
            var addressList = addresses.Split(',').ToList();

            var balances = await _walletService.GetWalletBalances(addressList);

            return balances != null ? Ok(balances) : StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}