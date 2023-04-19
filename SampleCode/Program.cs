using BitcoinCash;
using BitcoinCash.Models;
using Newtonsoft.Json;

var client = new BitcoinCashClient();

// create new wallet
var wallet = client.GetWallet();
Display("new wallet", wallet);


// get sample (existing) wallet
// to add to sample wallet balance, send BCH here: bitcoincash:qrkvgur05slhujutthx04mzc0mc483p7lqca5cr4qe
var privateKey = "L2ES4zx4AnTYTQWQnuUvzBEwsXdhaoLi9W15HgKw8osVaxeUHbcE";
wallet = client.GetWallet(privateKey);
Display("sample wallet", wallet);


// send 10% of wallet balance to sample address
var sampleAddress = Constants.DevAddress;
var sendAmount = (decimal)(wallet.Balance! * 0.1);
wallet.Send(sampleAddress, sendAmount, Currency.Satoshis);
Display("wallet after sat send", wallet);


// send $0.10 to sample address
sendAmount = 0.1m;
wallet.Send(sampleAddress, sendAmount, Currency.USDollars);
Display("wallet after usd send", wallet);


// get read-only wallet by public address
var samplePublicAddress = Constants.DevAddress;
var readOnlyWallet = client.GetWalletByAddress(samplePublicAddress);
Display("read-only wallet", readOnlyWallet);


// get current market value of BCH in default currency (USD)
var usdValue = client.GetFiatValue();
Display("USD value", usdValue);


Console.ReadLine();


void Display(string name, object obj)
{
    Console.WriteLine();
    Console.WriteLine($"{name}:");
    Console.WriteLine();
    Console.WriteLine(JsonConvert.SerializeObject(obj, Formatting.Indented));
    Console.WriteLine();
}