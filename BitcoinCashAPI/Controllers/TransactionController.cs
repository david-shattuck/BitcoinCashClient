using BitcoinCash.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BitcoinCash.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TransactionController(IKeyService keyService, ITransactionService transactionService) : ControllerBase
    {
        private readonly IKeyService _keyService = keyService;
        private readonly ITransactionService _transactionService = transactionService;

        [HttpGet]
        [Route("GetValidTxHashes")]
        public async Task<IActionResult> GetValidTxHashes(string key, string hashes)
        {
            if (!_keyService.IsValid(key))
                return StatusCode(StatusCodes.Status402PaymentRequired);

            var hashList = hashes.Split(',').Distinct().ToList();

            var validHashes = await _transactionService.GetValidTxHashes(hashList);

            return validHashes != null ? Ok(validHashes) : StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}