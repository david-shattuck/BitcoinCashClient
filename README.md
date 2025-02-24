[![Build Status](https://dev.azure.com/davidshattuck/BitcoinCashClient/_apis/build/status/david-shattuck.BitcoinCashClient?branchName=main)](https://dev.azure.com/davidshattuck/BitcoinCashClient/_build/latest?definitionId=5&branchName=main)

# About

BitcoinCashClient makes it trivially easy to integrate native (non-custodial) Bitcoin Cash transactions into any C# .NET application with internet access.

This library provides a simple interface enabling on-chain sending and receiving of Bitcoin Cash by abstracting away as much complexity as possible. Wallet (private key and public address) creation, fetching wallet info from a block explorer, fetching up-to-date price data, managing utxos (hashes and indexes), inputs and outputs, network fee calculation, transaction signing, and broadcasting to the network are ALL handled by this library behind the scenes.

Private keys are never exposed. [Don't trust. Verify.](https://github.com/david-shattuck/BitcoinCashClient)

## How to Use

### Instantiate

```csharp
using BitcoinCash;

var client = new BitcoinCashClient();
```

### Create new wallet

Generate a new Bitcoin Cash private key and its associated public address.

```csharp
var wallet = client.GetWallet();
```

### Get existing live wallet

Use a private key to get a live wallet. This operation retrieves all utxos, calculates the wallet balance (and fiat value), and enables sending funds.

```csharp
var privateKey = "<your-private-key>";
var wallet = await client.GetWallet(privateKey);
```

_Note: Again, the private key is never exposed. The key is used to compute the public address, which is then used to fetch the wallet info from a block explorer._

### Send

Send the specified amount of Bitcoin Cash to the specified address.

```csharp
// send one dollar's worth of BCH to the destination address
await wallet.Send("<destination-address>", 1m, Currency.USDollar);
```

### Send All

Send the entire wallet balance to the specified address.

```csharp
await wallet.SendAll("<destination-address>");
```

### Get existing read-only wallet

Use a public address to get a read-only wallet.

```csharp
var publicAddress = "<any-valid-bch-address>";
var wallet = await client.GetWalletByAddress(publicAddress);
```

## Links

Please view [the sample project](https://github.com/david-shattuck/BitcoinCashClient/blob/main/SampleCode/Program.cs) for simple live examples.

For a detailed explanation of the code, please see [this tutorial](https://read.cash/@thanah85/bitcoin-cash-payments-using-c-and-net-f1c4b00d).

## Version History

### 3.0

- API Keys

### 2.0

- Async/Await refactor

### 1.5

- .NET 8 and C# 12
- Send to many
- Unambiguous error states

### 1.4

- Convert address to CashAddr format
- Check if address is valid

### 1.3

- Mass wallet balance check

### 1.2

- Support for many fiat currencies
- Support for legacy addresses

### 1.1

- Get fiat value

### 1.0

- Create wallet
- Get live wallet by private key
- Get read-only wallet by public address
- Get list of live wallets by list of private keys
- Get list of read-only wallets by list of public addresses
- All wallets include balance in satoshis and current value in USD
- Send specified amount of USD or BCH to specified address
- Send entire wallet balance to specified address

## Feedback

BitcoinCashClient is released as open source under the [MIT license](https://github.com/david-shattuck/BitcoinCashClient/blob/main/LICENSE). Bug reports and contributions are welcome at [the GitHub repository](https://github.com/david-shattuck/BitcoinCashClient).
