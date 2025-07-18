using Nethereum.ABI.FunctionEncoding.Attributes;
using System.Numerics;

[FunctionOutput]
public class Score : IFunctionOutputDTO
    {
        [Parameter("uint256", "playerId", 1)]
        public BigInteger PlayerId { get; set; }
        [Parameter("string", "walletAddress", 2)]
        public string WalletAddress { get; set; }
        [Parameter("uint256", "coinAmount", 3)]
        public BigInteger CoinAmount { get; set; }
        [Parameter("uint256", "characterIndex", 4)]
        public BigInteger CharacterIndex { get; set; }
        [Parameter("uint", "exchangedAmount", 5)]
        public BigInteger exchangedAmount { get; set; }
        [Parameter("bool", "exists", 6)]
        public bool Exists { get; set; }
    }