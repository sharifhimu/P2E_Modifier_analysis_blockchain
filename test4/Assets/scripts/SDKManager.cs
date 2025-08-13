using UnityEngine;
using UnityEngine.SceneManagement;

using System;
using System.Threading.Tasks;
using System.Numerics;
//using System.Collections;
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

    private string tokenPairAbi = ABIManager.tokenpairABI;
    private string tokenPairAddress = ABIManager.tokenpairAddress;

    private string myTokenAbi = ABIManager.mytokenABI;
    private string myTokenAddress = ABIManager.mytokenAddress;

    public string token0Address;
    public string token1Address;


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

    async Task TokenFetch(){

        var pairContract = Web3.Eth.GetContract(ABIManager.tokenpairABI, ABIManager.tokenpairAddress);

        var token0Func = pairContract.GetFunction("token0");
        string token0Add = await token0Func.CallAsync<string>();

        var token1Func = pairContract.GetFunction("token1");
        string token1Add = await token1Func.CallAsync<string>();

        token0Address = token0Add;
        token1Address = token1Add;

        Debug.Log("token0 " + token0Address + " token1 " + token1Address );
    }

    private async Task<(double, double, double)> CallGetReserve(){

        var contract = Web3.Eth.GetContract(ABIManager.tokenpairABI, tokenPairAddress);
        var getReservesFunction = contract.GetFunction("getReserves");

        var reserves = await getReservesFunction.CallDeserializingToObjectAsync<Reserves>();

        double normalizedReserve0 = (double)reserves.Reserve0;
        double normalizedReserve1 = (double)reserves.Reserve1;

        double liquidity = (double)Math.Sqrt(normalizedReserve0 * normalizedReserve1 );
        
        Debug.Log($"Reserve0: {normalizedReserve0}");
        Debug.Log($"Reserve1: {normalizedReserve1}");

        return ( liquidity, normalizedReserve0, normalizedReserve1 );

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

     List<double> exampleLiquidity = new List<double>
    {
        Convert.ToUInt32("92bc7d4", 16),
        Convert.ToUInt32("92c1a36", 16),
        Convert.ToUInt32("92fcc87", 16),
        Convert.ToUInt32("9301f3b", 16),
        Convert.ToUInt32("931b8e8", 16),
        Convert.ToUInt32("82aea6d", 16),
        Convert.ToUInt32("82f1440", 16),
        Convert.ToUInt32("831981c", 16),
        Convert.ToUInt32("831981c", 16),
        Convert.ToUInt32("8322079", 16)
    }.Select(x => x / 1e6).ToList();


    private async void Start()
    {

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        Web3 = new Web3(rpcUrl);
        Debug.Log("Web3 connected from SDKManager to: " + rpcUrl + " web3 " + Web3);
        
        await TokenFetch();

        (double liquidity, double normalizedReserve0, double normalizedReserve1) = await CallGetReserve();

        // Now safely use result
        reserveValue = liquidity;
        reserve0 = normalizedReserve0;
        reserve1 = normalizedReserve1;

        string filepath = Application.persistentDataPath + "/clamp_range.csv";

        var (log, hasToday) = ReadLiquidityColumn(filepath, liquidity);
        List<double> logDoubles = log.Select(x => (double)x / 1e6).ToList();

        if (logDoubles.Count < 2 || logDoubles.Max() == logDoubles.Min())
        {
            Debug.LogWarning("Not enough file data. Using blockchain liquidity log instead.");
            logDoubles = exampleLiquidity;
        }
        else if (!hasToday)
        {
            Debug.Log("Today's liquidity not found in file. Adding to in-memory list.");
            logDoubles.Add((double)liquidity / 1e6);
        }
        else
        {
            Debug.Log("Today's liquidity already exists in file. Skipping add.");
        }

        double scaledLiquidity = (double)liquidity;
        double range = logDoubles.Max() - logDoubles.Min();
        double currentNormalized = (scaledLiquidity - logDoubles.Min()) / range;

        ClampResult bestClamp = SelectBestClampMethod(logDoubles, currentNormalized);

        Debug.Log("LogDoubles: " + string.Join(", ", logDoubles.Select(v => v.ToString("F2"))));
        Debug.Log($"Selected Clamp: {bestClamp.Name}");
        Debug.Log($"Final Normalized Clamp => Min: {bestClamp.NormMin:F4}, Mid: {bestClamp.NormMid:F4}, Max: {bestClamp.NormMax:F4}");

        minModifier = bestClamp.NormMin;
        midModifier = bestClamp.NormMid;
        maxModifier = bestClamp.NormMax;

        SaveClampRangeToFile(filepath, bestClamp, liquidity, DateTime.Now);
    }


    public static (List<double> liquidityValues, bool hasTodayLiquidity) ReadLiquidityColumn(string filePath, double currentLiquidity)
    {
        var liquidityValues = new List<double>();
        bool hasTodayLiquidity = false;

        try
        {
            if (!File.Exists(filePath))
            {
                Debug.LogWarning("CSV file not found: " + filePath);
                return (liquidityValues, false); // empty
            }

            var lines = File.ReadAllLines(filePath);

            if (lines.Length <= 1)
            {
                Debug.LogWarning("CSV file is empty or only has header: " + filePath);
                return (liquidityValues, false);
            }

            string todayDate = DateTime.Now.ToString("yyyy-MM-dd");

            // Skip header
            for (int i = 1; i < lines.Length; i++)
            {
                var parts = lines[i].Split(',');

                if (parts.Length < 5)
                {
                    Debug.LogWarning($"Line {i + 1} is malformed: {lines[i]}");
                    continue;
                }

                if (double.TryParse(parts[4], out double liquidity))
                {
                    liquidityValues.Add(liquidity);

                    if (parts[0].StartsWith(todayDate) &&
                        Math.Abs(liquidity - currentLiquidity) < 0.000001)
                    {
                        hasTodayLiquidity = true;
                    }
                }
                else
                {
                    Debug.LogWarning($"Liquidity value parse failed at line {i + 1}: {parts[4]}");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Error reading liquidity column: " + ex.Message);
        }

        return (liquidityValues, hasTodayLiquidity);
    }


    public void SaveClampRangeToFile(string filePath, ClampResult clamp, double liquidity, DateTime timestamp)
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
            string header = "Timestamp,Min,Mid,Max,liquidity";
            File.WriteAllText(filePath, header + "\n", Encoding.UTF8);
        }
        // Format row
        string row = string.Format(
            "{0},{1:F4},{2:F4},{3:F4},{4:G}",
            timestamp.ToString("yyyy-MM-dd HH:mm:ss"),
            clamp.NormMin,
            clamp.NormMid,
            clamp.NormMax,
            liquidity
        );
        // Append data
        File.AppendAllText(filePath, row + "\n", Encoding.UTF8);
        Debug.Log(" Clamp range saved: " + row);
    }



    // Update is called once per frame
    void Update()
    {
        
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

