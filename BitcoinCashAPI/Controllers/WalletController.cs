using BitcoinCash.API.Services.Interfaces;
using BitcoinCash.Models;
using Microsoft.AspNetCore.Mvc;

namespace BitcoinCash.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WalletController(IWalletService walletService) : ControllerBase
    {        
        private readonly IWalletService _walletService = walletService;

        [HttpGet]
        public List<Wallet> Get(string addresses, string currency)
        {
            var addressList = addresses.Split(',').ToList();

            return _walletService.GetWalletInfo(addressList, currency);
        }

        [HttpPost]
        [Route("GetBalances")]
        public List<KeyValuePair<string, long>> GetBalances([FromForm] string addresses)
        {
            var addressList = addresses.Split(',').ToList();

            return _walletService.GetWalletBalances(addressList);
        }
    }
}