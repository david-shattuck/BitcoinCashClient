using BitcoinCash;
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
var sampleAddress = "bitcoincash:qpd5sv0st7v6svn59g0fdj82kze4f87wggjxxqznzu";
wallet.Send(sampleAddress, (decimal)(wallet.Balance! * 0.1));
Display("wallet after send", wallet);


Console.ReadLine();


void Display(string name, object obj)
{
    Console.WriteLine();
    Console.WriteLine();
    Console.WriteLine($"{name}:");
    Console.WriteLine();
    Console.WriteLine(JsonConvert.SerializeObject(obj));
    Console.WriteLine();
}