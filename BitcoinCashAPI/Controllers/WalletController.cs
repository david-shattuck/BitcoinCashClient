using BitcoinCash.API.Services.Interfaces;
using BitcoinCash.Models;
using Microsoft.AspNetCore.Mvc;

namespace BitcoinCash.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WalletController : ControllerBase
    {        
        private readonly IWalletService _walletService;

        public WalletController(IWalletService walletService)
        {
            _walletService = walletService;
        }

        [HttpGet]
        public List<Wallet> Get(string addresses, string currency)
        {
            var addressList = addresses.Split(',').ToList();

            return _walletService.GetWalletInfo(addressList, currency);
        }

        [HttpGet]
        [Route("GetBalances")]
        public List<KeyValuePair<string, long>> GetBalances(string addresses)
        {
            var addressList = addresses.Split(',').ToList();

            return _walletService.GetWalletBalances(addressList);
        }
    }
}