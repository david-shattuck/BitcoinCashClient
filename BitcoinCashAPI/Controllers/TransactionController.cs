using BitcoinCash.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BitcoinCash.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TransactionController : ControllerBase
    {        
        private readonly ITransactionService _transactionService;

        public TransactionController(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        [HttpGet]
        [Route("GetValidTxHashes")]
        public List<string> GetValidTxHashes(string hashes)
        {
            var hashList = hashes.Split(',').Distinct().ToList();

            return _transactionService.GetValidTxHashes(hashList);
        }
    }
}