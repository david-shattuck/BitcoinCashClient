using BitcoinCash.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BitcoinCash.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TransactionController(ITransactionService transactionService) : ControllerBase
    {        
        private readonly ITransactionService _transactionService = transactionService;

        [HttpGet]
        [Route("GetValidTxHashes")]
        public IActionResult GetValidTxHashes(string hashes)
        {
            var hashList = hashes.Split(',').Distinct().ToList();

            var validHashes = _transactionService.GetValidTxHashes(hashList);

            return validHashes != null ? Ok(validHashes) : StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}