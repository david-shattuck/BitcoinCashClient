using BitcoinCash.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BitcoinCash.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WalletController(IKeyService keyService, IWalletService walletService) : ControllerBase
    {
        private readonly IKeyService _keyService = keyService;
        private readonly IWalletService _walletService = walletService;

        [HttpGet]
        public async Task<IActionResult> Get(string key, string addresses, string currency)
        {
            if (!_keyService.IsValid(key))
                return StatusCode(StatusCodes.Status402PaymentRequired);

            await _keyService.CheckForPayments();

            var addressList = addresses.Split(',').ToList();

            var wallets = await _walletService.GetWalletInfo(addressList, currency);

            return wallets != null ? Ok(wallets) : StatusCode(StatusCodes.Status500InternalServerError);
        }

        [HttpPost]
        [Route("GetBalances")]
        public async Task<IActionResult> GetBalances(string key, [FromForm] string addresses)
        {
            if (!_keyService.IsValid(key))
                return StatusCode(StatusCodes.Status402PaymentRequired);

            var addressList = addresses.Split(',').ToList();

            var balances = await _walletService.GetWalletBalances(addressList);

            return balances != null ? Ok(balances) : StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}