using UnityEngine;
using UnityEngine.SceneManagement;

using System;
using System.Threading.Tasks;
using System.Numerics;
// using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

using Nethereum.Web3;
using Nethereum.Hex.HexTypes;
using Nethereum.JsonRpc.Client;
using Nethereum.ABI.FunctionEncoding.Attributes;

using UnityEngine.Networking;

using Newtonsoft.Json.Linq;

using Nethereum.Contracts;




public class SDKManager : MonoBehaviour
{
    public static SDKManager Instance{ get; private set; }
    public Web3 Web3 { get; private set; } 
    [SerializeField][HideInInspector] private string rpcUrl = "http://192.168.100.60:8545"; 

    string testcontractAddress = "0xA4e8331294C96EBcC29C6A2d577aB39E22BdAe8e";
    string testabi = @"[{""inputs"":[],""stateMutability"":""nonpayable"",""type"":""constructor""},{""anonymous"":false,""inputs"":[{""indexed"":false,""internalType"":""address"",""name"":""sender"",""type"":""address""},{""indexed"":false,""internalType"":""uint256"",""name"":""playerId"",""type"":""uint256""},{""indexed"":false,""internalType"":""string"",""name"":""wallet"",""type"":""string""},{""indexed"":false,""internalType"":""uint256"",""name"":""coin"",""type"":""uint256""},{""indexed"":false,""internalType"":""uint256"",""name"":""character"",""type"":""uint256""}],""name"":""ScoreSubmitted"",""type"":""event""},{""inputs"":[],""name"":""ethPrice"",""outputs"":[{""internalType"":""uint256"",""name"":"""",""type"":""uint256""}],""stateMutability"":""view"",""type"":""function"",""constant"":true},{""inputs"":[],""name"":""lastLiquidityUpdate"",""outputs"":[{""internalType"":""uint256"",""name"":"""",""type"":""uint256""}],""stateMutability"":""view"",""type"":""function"",""constant"":true},{""inputs"":[{""internalType"":""uint256"",""name"":"""",""type"":""uint256""}],""name"":""liquidityLog"",""outputs"":[{""internalType"":""uint256"",""name"":"""",""type"":""uint256""}],""stateMutability"":""view"",""type"":""function"",""constant"":true},{""inputs"":[],""name"":""maxClamp"",""outputs"":[{""internalType"":""uint256"",""name"":"""",""type"":""uint256""}],""stateMutability"":""view"",""type"":""function"",""constant"":true},{""inputs"":[],""name"":""midClamp"",""outputs"":[{""internalType"":""uint256"",""name"":"""",""type"":""uint256""}],""stateMutability"":""view"",""type"":""function"",""constant"":true},{""inputs"":[],""name"":""minClamp"",""outputs"":[{""internalType"":""uint256"",""name"":"""",""type"":""uint256""}],""stateMutability"":""view"",""type"":""function"",""constant"":true},{""inputs"":[{""internalType"":""uint256"",""name"":"""",""type"":""uint256""}],""name"":""playerAddresses"",""outputs"":[{""internalType"":""address"",""name"":"""",""type"":""address""}],""stateMutability"":""view"",""type"":""function"",""constant"":true},{""inputs"":[{""internalType"":""address"",""name"":"""",""type"":""address""}],""name"":""playerScores"",""outputs"":[{""internalType"":""uint256"",""name"":""playerId"",""type"":""uint256""},{""internalType"":""string"",""name"":""walletAddress"",""type"":""string""},{""internalType"":""uint256"",""name"":""coinAmount"",""type"":""uint256""},{""internalType"":""uint256"",""name"":""characterIndex"",""type"":""uint256""},{""internalType"":""uint256"",""name"":""exchangedAmount"",""type"":""uint256""},{""internalType"":""bool"",""name"":""exists"",""type"":""bool""}],""stateMutability"":""view"",""type"":""function"",""constant"":true},{""inputs"":[],""name"":""hello"",""outputs"":[{""internalType"":""string"",""name"":"""",""type"":""string""}],""stateMutability"":""pure"",""type"":""function"",""constant"":true},{""inputs"":[{""internalType"":""uint256"",""name"":""_price"",""type"":""uint256""}],""name"":""setPrice"",""outputs"":[],""stateMutability"":""nonpayable"",""type"":""function""},{""inputs"":[{""internalType"":""uint256"",""name"":""playerId"",""type"":""uint256""},{""internalType"":""string"",""name"":""add"",""type"":""string""},{""internalType"":""uint256"",""name"":""coinAmount"",""type"":""uint256""},{""internalType"":""uint256"",""name"":""characterIndex"",""type"":""uint256""}],""name"":""Sendscore"",""outputs"":[],""stateMutability"":""nonpayable"",""type"":""function""},{""inputs"":[{""internalType"":""uint256"",""name"":""_newCoinAmount"",""type"":""uint256""},{""internalType"":""uint256"",""name"":""_newCharacterIndex"",""type"":""uint256""},{""internalType"":""uint256"",""name"":""_exchangedAmount"",""type"":""uint256""}],""name"":""updateScoreByPlayerId"",""outputs"":[],""stateMutability"":""nonpayable"",""type"":""function""},{""inputs"":[{""internalType"":""uint256"",""name"":""_amount"",""type"":""uint256""}],""name"":""setExchangeAmount"",""outputs"":[],""stateMutability"":""nonpayable"",""type"":""function""},{""inputs"":[],""name"":""getAllData"",""outputs"":[{""internalType"":""uint256"",""name"":"""",""type"":""uint256""},{""components"":[{""internalType"":""uint256"",""name"":""playerId"",""type"":""uint256""},{""internalType"":""string"",""name"":""walletAddress"",""type"":""string""},{""internalType"":""uint256"",""name"":""coinAmount"",""type"":""uint256""},{""internalType"":""uint256"",""name"":""characterIndex"",""type"":""uint256""},{""internalType"":""uint256"",""name"":""exchangedAmount"",""type"":""uint256""},{""internalType"":""bool"",""name"":""exists"",""type"":""bool""}],""internalType"":""struct TestContract.Score[]"",""name"":"""",""type"":""tuple[]""}],""stateMutability"":""view"",""type"":""function"",""constant"":true},{""inputs"":[{""internalType"":""address"",""name"":""_wallet"",""type"":""address""}],""name"":""getPlayerData"",""outputs"":[{""internalType"":""uint256"",""name"":""playerId"",""type"":""uint256""},{""internalType"":""string"",""name"":""walletAddress"",""type"":""string""},{""internalType"":""uint256"",""name"":""coinAmount"",""type"":""uint256""},{""internalType"":""uint256"",""name"":""characterIndex"",""type"":""uint256""},{""internalType"":""uint256"",""name"":""exchangedAmount"",""type"":""uint256""},{""internalType"":""bool"",""name"":""exists"",""type"":""bool""}],""stateMutability"":""view"",""type"":""function"",""constant"":true},{""inputs"":[],""name"":""getClamps"",""outputs"":[{""internalType"":""uint256"",""name"":"""",""type"":""uint256""},{""internalType"":""uint256"",""name"":"""",""type"":""uint256""},{""internalType"":""uint256"",""name"":"""",""type"":""uint256""}],""stateMutability"":""view"",""type"":""function"",""constant"":true},{""inputs"":[{""internalType"":""uint256"",""name"":""_min"",""type"":""uint256""},{""internalType"":""uint256"",""name"":""_mid"",""type"":""uint256""},{""internalType"":""uint256"",""name"":""_max"",""type"":""uint256""}],""name"":""setModifiers"",""outputs"":[],""stateMutability"":""nonpayable"",""type"":""function""},{""inputs"":[{""internalType"":""uint256"",""name"":""currentLiquidity"",""type"":""uint256""}],""name"":""updateLiquidity"",""outputs"":[],""stateMutability"":""nonpayable"",""type"":""function""},{""inputs"":[],""name"":""getLiquidityLog"",""outputs"":[{""internalType"":""uint256[]"",""name"":"""",""type"":""uint256[]""}],""stateMutability"":""view"",""type"":""function"",""constant"":true}]";

    [HideInInspector] public string walletAddress = "0x4aB5E0D87B8f27036a472ACAe9D7FbD34af4F51c";
    [HideInInspector] public string walletAddress2 = "0xEeC5bD1f87A918E56644DD60c09848B88Ec410e7";
    [HideInInspector] public string walletAddress3 = "0x5cdC304b37cF49A5FF9cbA6C59c8A8529F443164";
    [HideInInspector] public string walletAddress4 = "0x7BE00eD244D11b14ff184a0dA503A9A4659D3A41";
    [HideInInspector] public string walletAddress5 = "0xD4c9b0818E0FEcCa7aC2096749eDA754a9c9c857";

    public int selectedCharacterIndex = 0; // default to first character
    public BigInteger totalCoin = 0;
    public int playerId = 1;

    public double minModifier = 0.0f;
    public double maxModifier = 0.0f;
    public double midModifier = 0.0f;
    
    [HideInInspector]public double priceUsd = 0;
    [HideInInspector]public decimal marketPrice = 0;
    [HideInInspector]public double TotalExchangeableToken = 0;
    [HideInInspector]public double TotalToken = 0;

    [HideInInspector]public double modifier1 = 0;
    [HideInInspector]public double modifierOne = 0;
    [HideInInspector]public double modifierTwo = 0;
    [HideInInspector]public double modifierThree = 0;

    [HideInInspector]public double reserveValue = 0;
    [HideInInspector]public double reserve0 = 0;
    [HideInInspector]public double reserve1 = 0;

    [HideInInspector] public string testRpcUrl = "https://mainnet.infura.io/v3/3a57c44201504fff96c533b1b2c3515f";
    [HideInInspector] public string testAbi = @"[
        {
            ""constant"": true,
            ""inputs"": [],
            ""name"": ""getReserves"",
            ""outputs"": [
                { ""internalType"": ""uint112"", ""name"": ""reserve0"", ""type"": ""uint112"" },
                { ""internalType"": ""uint112"", ""name"": ""reserve1"", ""type"": ""uint112"" },
                { ""internalType"": ""uint32"", ""name"": ""blockTimestampLast"", ""type"": ""uint32"" }
            ],
            ""payable"": false,
            ""stateMutability"": ""view"",
            ""type"": ""function""
        }
    ]";

   // slp/weth
    [HideInInspector] public string pairContractAddress = "0x3dDBbFd2CF0120F6E98Ea0D44AeD3475C385F0E9";
    [HideInInspector] public string ohlcvUrl = "https://api.geckoterminal.com/api/v2/networks/ronin/pools/0x306a28279d04a47468ed83d55088d0dcd1369294/ohlcv";
    [HideInInspector] public string dexscreenerApi = "https://api.coingecko.com/api/v3/simple/price?ids=smooth-love-potion&vs_currencies=usd";


    // GST/WSOL not right informations
    // [HideInInspector] public string pairContractAddress = "0x3dDBbFd2CF0120F6E98Ea0D44AeD3475C385F0E9";
    // [HideInInspector] public string ohlcvUrl = "https://api.geckoterminal.com/api/v2/networks/solana/pools/2ko9dfZVkCehcw7iY8zyQ5qA5YDLzyPh2etZgJrsCufk/ohlcv";
    // [HideInInspector] public string dexscreenerApi = "https://api.coingecko.com/api/v3/simple/price?ids=smooth-love-potion&vs_currencies=usd";


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        Web3 = new Web3(rpcUrl); // Create Web3 once here
        Debug.Log("Web3 connected to: " + rpcUrl + " web3 " + Web3 );
    }

    private async Task<(double, double, double)> CallGetReserve(){

        var web3 = new Web3(testRpcUrl);
        var contract = web3.Eth.GetContract(testAbi, pairContractAddress);
        var getReservesFunction = contract.GetFunction("getReserves");

        var reserves = await getReservesFunction.CallDeserializingToObjectAsync<Reserves>();

        var (token0, token1) =   await checkDecimals(web3);

        // Debug.Log("tokenDecimals " + (  token0, token1 ) );

        double normalizedReserve0 = (double)reserves.Reserve0 / Math.Pow(10, token0);
        double normalizedReserve1 = (double)reserves.Reserve1 / Math.Pow(10, token1);

        double liquidity = (double)Math.Sqrt(normalizedReserve0 * normalizedReserve1 );
        // onchainLiquidity = liquidity;
            Debug.Log($"Reserve0: {normalizedReserve0}");
            Debug.Log($"Reserve1: {normalizedReserve1}");
        // Debug.Log($"liquidity from bonk/weth chain: {liquidity}");

        return ( liquidity, normalizedReserve0, normalizedReserve1 );

    }

    private async Task<bool> pushLiquidity( double newLiquidityValue ){

        Debug.Log("address " + testcontractAddress + " abi " + testabi );

        var web3 = Web3;
        var contract = web3.Eth.GetContract(testabi, testcontractAddress);
        var pushFunction = contract.GetFunction("updateLiquidity");

        try{

            var accountAddress = walletAddress;
            BigInteger liquidityScaled = new BigInteger(Math.Round(newLiquidityValue * 1e6));
            Debug.Log("liquidityScaled " + liquidityScaled );
            var result = await pushFunction.SendTransactionAsync(
                from: accountAddress,                     
                gas: new HexBigInteger(900000),
                value: null,
                functionInput: new object[]
                {
                    liquidityScaled
                }
            );

            Debug.Log("Liquidity pushed tx: " + result);

        } 
        catch(RpcResponseException ex){
            Debug.LogError("Transaction failed: " + ex.Message);
        }
        
        return true;

    }

    public async Task<List<uint>> GetLiquidityLog()
    {
        
        var contract = Web3.Eth.GetContract(testabi, testcontractAddress);
        var getLogFunction = contract.GetFunction("getLiquidityLog");

        // Ekhon ekbar e pura array fetch
        var liquidityArray = await getLogFunction.CallAsync<List<uint>>();

        return liquidityArray;
    }

    public static (double mean, double stdDev) CalculateSMA(List<double> liquidityData){
            double mean = liquidityData.Average();
            double variance = liquidityData.Sum(l => Math.Pow(l - mean, 2)) / liquidityData.Count;
            double stdDev = Math.Sqrt(variance);

            return ( mean, stdDev );
    }

    public static (double mean, double stdDev) CalculateEMA(List<double> liquidityData, int period)
    {
        List<double> emaValues = new List<double>();

        if (liquidityData == null || liquidityData.Count < period)
            throw new ArgumentException("Not enough data for EMA calculation.");

        double multiplier = 2.0 / (period + 1);
        double sma = liquidityData.Take(period).Average();  // initial EMA from SMA
        emaValues.Add(sma);

        for (int i = period; i < liquidityData.Count; i++)
        {
            double ema = (liquidityData[i] - emaValues.Last()) * multiplier + emaValues.Last();
            emaValues.Add(ema);
        }

        double mean = emaValues.Last();
        ( double SMAMean, double SMAstdDev ) = CalculateSMA(liquidityData.Skip(liquidityData.Count - period).ToList());


        return ( mean, SMAstdDev );
    }


    private (double midWeight, double tightWeight, double wideWeight) CalculateWeights(int count)
    {
        // If data is low, prioritize wide range; else, prioritize tight clamp
        double factor = Mathf.Clamp01(count / 30f); // Normalize count to [0, 1]
        double midWeight = 0.3 + 0.2 * (1 - factor);  // More weight when data is low
        double tightWeight = 0.4 * factor;
        double wideWeight = 1.0 - tightWeight;

        return (midWeight, tightWeight, wideWeight);
    }


    ClampResult EvaluateClampMethod(string methodName, ClampResult clamp, List<double> logDoubles, double currentNormalized)
    {
        double range = logDoubles.Max() - logDoubles.Min();
        if (range < 1e-6) range = 1e-6; // Prevent divide-by-zero

        // double wideness = clamp.Max - clamp.Min;
        // double midError = Math.Abs(currentNormalized - clamp.Mid);
        // double tightness = 1.0 - wideness;

        // var (midWeight, tightWeight, wideWeight) = CalculateWeights(logDoubles.Count);

        // double errorScore = (midError * midWeight) + (tightness * tightWeight) + ((1.0 - wideness) * wideWeight);

        return new ClampResult
        {
            Name = methodName,
            Min = clamp.Min,
            Mid = clamp.Mid,
            Max = clamp.Max,
            // Error = errorScore,
            NormMin = (clamp.Min - logDoubles.Min()) / range,
            NormMid = (clamp.Mid - logDoubles.Min()) / range,
            NormMax = (clamp.Max - logDoubles.Min()) / range
        };
    }


    private ClampResult SelectBestClampMethod(List<double> logDoubles, double currentNormalized)
    {

        var (emaMean, emaStdDev) = CalculateEMA(logDoubles, logDoubles.Count);
        var emaClamp = new ClampResult { Min = emaMean - emaStdDev, Max = emaMean + emaStdDev, Mid = emaMean };

        List<ClampResult> clampResults = new List<ClampResult>
        {
            // EvaluateClampMethod("SMA", smaClamp, logDoubles, currentNormalized),
            EvaluateClampMethod("EMA", emaClamp, logDoubles, currentNormalized)
        };

        foreach (var c in clampResults)
        {
            Debug.Log($"Clamp Method {c.Name} => Min: {c.Min:F4}, Mid: {c.Mid:F4}, Max: {c.Max:F4}, NormMin: {c.NormMin:F4}, NormMid: {c.NormMid:F4}, NormMax: {c.NormMax:F4}, Error: {c.Error:F4}");
        }

       // return clampResults.OrderBy(c => c.Error).First();
        return clampResults.First();
    }



    private async void Start()
    {
        var (liquidity, normalizedReserve0, normalizedReserve1) = await CallGetReserve();
        reserveValue = liquidity;
        reserve0 = normalizedReserve0;
        reserve1 = normalizedReserve1;

        if (!await pushLiquidity(liquidity))
            return;

        List<uint> log = await GetLiquidityLog();
        List<double> logDoubles = log.Select(x => (double)x/1e6).ToList();

        if (logDoubles.Count < 2 || logDoubles.Max() == logDoubles.Min())
        {
            Debug.LogWarning("Not enough data or range is zero.");
            return;
        }

        double scaledLiquidity = liquidity;
        double range = logDoubles.Max() - logDoubles.Min();
        double currentNormalized = (scaledLiquidity - logDoubles.Min()) / range;

        ClampResult bestClamp = SelectBestClampMethod(logDoubles, currentNormalized);

        Debug.Log("LogDoubles: " + string.Join(", ", logDoubles.Select(v => v.ToString("F2"))));
        Debug.Log($"Selected Clamp: {bestClamp.Name}");
        Debug.Log($"Final Normalized Clamp => Min: {bestClamp.NormMin:F4}, Mid: {bestClamp.NormMid:F4}, Max: {bestClamp.NormMax:F4}");

        minModifier = bestClamp.NormMin;
        midModifier = bestClamp.NormMid;
        maxModifier = bestClamp.NormMax;

        string path = Application.persistentDataPath + "/clamp_range.csv";
        SaveClampRangeToFile(path, bestClamp, DateTime.Now);

    }

    public void SaveClampRangeToFile(string filePath, ClampResult clamp, DateTime timestamp)
    {
        // Ensure directory exists
        Directory.CreateDirectory(Path.GetDirectoryName(filePath));

       // Skip saving if already saved today
        if (FileChecker.AlreadySavedToday(filePath))
        {
            Debug.Log("Clamp already saved today. Skipping save.");
            return;
        }

        // Add header if file doesn't exist
        if (!File.Exists(filePath))
        {
            string header = "Timestamp,Min,Mid,Max";
            File.WriteAllText(filePath, header + "\n", Encoding.UTF8);
        }

        // Format row
        string row = string.Format(
            "{0},{1:F4},{2:F4},{3:F4}",
            timestamp.ToString("yyyy-MM-dd HH:mm:ss"),
            clamp.NormMin,
            clamp.NormMid,
            clamp.NormMax
        );

        // Append data
        File.AppendAllText(filePath, row + "\n", Encoding.UTF8);
        Debug.Log(" Clamp range saved: " + row);
    }



    // Update is called once per frame
    void Update()
    {
        
    }

    private async Task<(int, int)> checkDecimals(Web3 web3){

        // Create contract query handlers
        var token0Handler = web3.Eth.GetContractQueryHandler<Token0Function>();
        var token1Handler = web3.Eth.GetContractQueryHandler<Token1Function>();

        // Query token0 and token1 addresses
        var token0Address = await token0Handler
            .QueryAsync<string>(pairContractAddress, new Token0Function());

        var token1Address = await token1Handler
            .QueryAsync<string>(pairContractAddress, new Token1Function());

        // Debug.Log($"Token0 Address: {token0Address}");
        // Debug.Log($"Token1 Address: {token1Address}");

        var decimalsHandler = web3.Eth.GetContractQueryHandler<DecimalsFunction>();

        var token0Decimals = await decimalsHandler
        .QueryAsync<byte>(token0Address, new DecimalsFunction());

        // Query decimals for token1
        var token1Decimals = await decimalsHandler
            .QueryAsync<byte>(token1Address, new DecimalsFunction());

        // Debug.Log($"Token0 Decimals: {token0Decimals}");
        // Debug.Log($"Token1 Decimals: {token1Decimals}");

        return (token0Decimals, token1Decimals);

    }


}

public class ClampResult
{
    public string Name;
    public double Min;
    public double Mid;
    public double Max;
    public double Error;

    public double NormMin;
    public double NormMid;
    public double NormMax;
}

[FunctionOutput]
public class ClampOutputDTO : IFunctionOutputDTO
{
    [Parameter("uint256", "minClamp", 1)]
    public uint minClamp { get; set; }

    [Parameter("uint256", "midClamp", 2)]
    public uint midClamp { get; set; }

    [Parameter("uint256", "maxClamp", 3)]
    public uint maxClamp { get; set; }
}


// This will get token0 address
[Function("token0", "address")]
public class Token0Function : FunctionMessage { }

// This will get token1 address
[Function("token1", "address")]
public class Token1Function : FunctionMessage { }

[Function("decimals", "uint8")]
public class DecimalsFunction : FunctionMessage { }

