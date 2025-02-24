using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace BitcoinCash.API.Models
{
    public class CacheKeys
    {
        public const string BchPrice = "BchPrice";
        public const string UnlockTime = "UnlockTime";
        public const string NextPaymentCheck = "NextPaymentCheck";
        public const string InvalidAddresses = "InvalidAddresses";
        public const string RecentAttempts = "RecentKeyCreationAttempts";
    }

    public class BchTransactionTypes
    {
        public const int FundKey = 1;
        public const int BuyRequests = 2;
    }

    public class BchTxFromToTypes
    {
        public const int External = 1;
        public const int Key = 2;
    }

    public class BchTransactionStatus
    {
        public const int Pending = 1;
        public const int Confirmed = 2;
        public const int Failed = 3;
    }

    public class Environments
    {
        public const string Development = "Development";
        public const string Production = "Production";
    }

    public class Addresses
    {
        public const string DevWallet = "bitcoincash:qrz5ua29hh767ygl42enpggsfj4dgql2zgqudy96rt";
    }    
}
