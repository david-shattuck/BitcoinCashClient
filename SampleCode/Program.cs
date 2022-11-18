using BitcoinCash;
using Newtonsoft.Json;

var client = new BitcoinCashClient();

// create new wallet
var wallet = client.GetWallet();
Display("New Wallet", wallet);


// get existing wallet
// to add to sample balance, send BCH here: bitcoincash:qrkvgur05slhujutthx04mzc0mc483p7lqca5cr4qe
var privateKey = "L2ES4zx4AnTYTQWQnuUvzBEwsXdhaoLi9W15HgKw8osVaxeUHbcE";

wallet = client.GetWallet(privateKey);
Display("Existing Wallet", wallet);



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