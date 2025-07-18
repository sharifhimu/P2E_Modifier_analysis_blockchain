using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

using System;
using System.Numerics;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;

using Nethereum.Web3;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

using Newtonsoft.Json.Linq;

using Nethereum.JsonRpc.Client;



public class LiquidityCalculation : MonoBehaviour
{

    // private double apiMaxLiquidity = 0;
    // private double onchainLiquidity = 0;
    private double finalMaxLiquidity = 0;

    void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("OnSceneLoaded");
        if (scene.name == "Scene4")  // or use buildIndex
        {
            Debug.Log("Scene4 loaded again, running logic...");
            StartCoroutine(RunLiquidityLogic());
        }
    }

    private IEnumerator RunLiquidityLogic()
    {
        yield return null;

        Debug.Log("Run your logic here");
    }

    async void Start()
    {
        // Debug.Log("called");
         var ( modifier1, modifier2, marketPrice )  = await CalculateModifier12();
         SDKManager.Instance.marketPrice = marketPrice;

         var modifier3 = await CalculateModifier3();
         
         SDKManager.Instance.modifierOne = modifier1;
         SDKManager.Instance.modifierTwo = modifier2;
         SDKManager.Instance.modifierThree = modifier3;

         SDKManager.Instance.modifier1 = HybridFinalModifier( modifier1, modifier2, modifier3 );
         // Debug.Log($"modifier: {modifierVal}");
    }

    void Update()
    {
        
    }


    private async Task<(double, double, decimal)> CalculateModifier12(){
        var ( reserve0, reserve1, maxLiquidity, smallAvgPrice, bigAvgPrice ) = await CalculationsFromApiData();
        double liquidity = reserve0;
        double minModifier = SDKManager.Instance.minModifier;
        double maxModifier = SDKManager.Instance.maxModifier;
        decimal marketPrice = (decimal)(reserve1/reserve0);

        if( reserve0 > reserve1 ) {
            liquidity = reserve0;
        } else {
            liquidity = reserve1;
            marketPrice = (decimal)(reserve0/reserve1);
        }

        // modifier 1 calculate
        var modifier1 = Math.Clamp( liquidity/maxLiquidity, minModifier, maxModifier ); 

        // modifier 2 calculate 
        float marketPriceFloat = (float)marketPrice;
        var modifier2 = marketPrice > bigAvgPrice ? SDKManager.Instance.minModifier : 
                            ( marketPrice < smallAvgPrice ? SDKManager.Instance.maxModifier : SDKManager.Instance.midModifier );

        Debug.Log($"liquidity: {liquidity}, maxLiquidity: {maxLiquidity}, reserve0: {reserve0}, reserve1: {reserve1}");
        Debug.Log($"liquidity/maxLiquidity: {liquidity/maxLiquidity}");
        // Debug.Log($"marketPrice: { marketPrice } marketPriceFloat: {marketPriceFloat} ");
        // Debug.Log($"smallAvgPrice: { smallAvgPrice } bigAvgPrice: {bigAvgPrice} ");

        Debug.Log($" modifier1: {modifier1}");
        Debug.Log($"modifier2: {modifier2}");

        return ( modifier1, modifier2, marketPrice );

    }

    private async Task<double> CalculateModifier3(){
        
        var web3 = SDKManager.Instance.Web3;
        var contract = web3.Eth.GetContract(SDKManager.Instance.abi, SDKManager.Instance.contractAddress);
        var SendScoreFunction = contract.GetFunction("getAllData");

        try{

            var result = await SendScoreFunction.CallDeserializingToObjectAsync<GetAllDataOutputDTO>();
            
            foreach (var score in result.Scores)
            {
                // Debug.Log($"PlayerID: {score.PlayerId}, Wallet: {score.WalletAddress}, Coins: {score.CoinAmount}, exchange: { score.exchangedAmount }, Character: {score.CharacterIndex}, Exists: {score.Exists} ");

                SDKManager.Instance.TotalExchangeableToken += (double)score.exchangedAmount;
                SDKManager.Instance.TotalToken += (double)score.CoinAmount;
            }

            // modifier 3 calculation
            double ratio = SDKManager.Instance.TotalExchangeableToken / SDKManager.Instance.TotalToken;            
            var modifier3 = (ratio < 0.2) ? SDKManager.Instance.maxModifier : 
                                (ratio > 0.5) ? SDKManager.Instance.minModifier : SDKManager.Instance.midModifier;
            
            // modifier 4 calculation
            // var modifier4 = SDKManager.Instance.TotalToken > 1000000 ? 0.6f : ( SDKManager.Instance.TotalToken < 300000 ? 1f : 0.5f );

            // var finalModifierFrom34 = Math.Min( modifier3, modifier4 );

            Debug.Log( $" modifier3: {modifier3} " );

            return modifier3;

        } 
        catch(RpcResponseException ex){
            Debug.LogError("Transaction failed: " + ex.Message);
            return 0.0;
        }
    }

    public double HybridFinalModifier(double m1, double m2, double m3)
    {
        // Step 1: Calculate spread
        double maxMod = Math.Max(m1, Math.Max(m2, m3));
        double minMod = Math.Min(m1, Math.Min(m2, m3));
        
        double mid = (maxMod + minMod) / 2.0;


        double d1 = Math.Abs(m1 - mid);
        double d2 = Math.Abs(m2 - mid);
        double d3 = Math.Abs(m3 - mid);

        double ε = 0.001;
        double w1 = 1.0 / (d1 + ε); // Add small ε to avoid divide-by-zero
        double w2 = 1.0 / (d2 + ε);
        double w3 = 1.0 / (d3 + ε);

        double total = w1 + w2 + w3;
        w1 /= total;
        w2 /= total;
        w3 /= total;

        double final = m1 * w1 + m2 * w2 + m3 * w3;
        double finalModifier = Math.Round(final, 3);
        
        Debug.Log("finalModifier " + finalModifier );

        return finalModifier;

    }


    private async Task<( double, double, double, decimal, decimal )> CalculationsFromApiData()
    {
        // modifier 1 related calculation
        var ( reserveValue, reserve0, reserve1 ) = await CallGetReserve(); // live on chain data

        ( double dayLiquidity, decimal smallDayPrice, decimal bigDayPrice ) = await OHLCVData("day");
        ( double hourLiquidity, decimal smallHourPrice, decimal bigHourPrice ) = await OHLCVData("hour");
        ( double minuteLiquidity, decimal smallMinPrice, decimal bigMinPrice ) = await OHLCVData("minute");

        double finalOHLCVMaxLiquidity = (dayLiquidity * 0.5) + (hourLiquidity * 0.3) + (minuteLiquidity * 0.2);
        finalMaxLiquidity = Math.Max(finalOHLCVMaxLiquidity, reserveValue);
        //

        // modifier 2 related calculation
        decimal smallerPrice = Math.Min(Math.Min(smallDayPrice, smallHourPrice), smallMinPrice);
        decimal biggerPrice = Math.Max(Math.Max(bigDayPrice, bigHourPrice), bigMinPrice);

        Debug.Log($"daily liquidity: {dayLiquidity}, hourly liquidity: {hourLiquidity}, minute-based liquidity: {minuteLiquidity}");
        Debug.Log($"daily price : smaller: {smallDayPrice}, bigger: {bigDayPrice}");
        Debug.Log($"hourly price : smaller: {smallHourPrice}, bigger: {bigHourPrice}");
        Debug.Log($"minute-based price : smaller: {smallMinPrice}, bigger: {bigMinPrice}");
        
        // Debug.Log($"smallDayPrice: {smallDayPrice} bigDayPrice: {bigDayPrice} smallHourPrice: {smallHourPrice} bigHourPrice: {bigHourPrice} smallMinPrice: {smallMinPrice} bigMinPrice: {bigMinPrice} ");

        //
        return ( reserve0, reserve1, finalMaxLiquidity, smallerPrice, biggerPrice ); 
    }



    private async Task<(double, double, double)> CallGetReserve(){

        // await GetPairAddress();

        var web3 = new Web3(SDKManager.Instance.testRpcUrl);
        var contract = web3.Eth.GetContract(SDKManager.Instance.testAbi, SDKManager.Instance.pairContractAddress);
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


    private async Task<(double, decimal, decimal )> OHLCVData(string timeframe)
    {
        double maxEstimatedLiquidity = 0;

        string url = $"{SDKManager.Instance.ohlcvUrl}/{timeframe}";
        UnityWebRequest www = UnityWebRequest.Get(url);
        await www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            string json = www.downloadHandler.text;
            var jobject = JObject.Parse(json);
            JArray ohlcvList = (JArray)jobject["data"]?["attributes"]?["ohlcv_list"];

            List<decimal> closingPrices = new List<decimal>();

            foreach (JArray entry in ohlcvList)
            {
                long timestamp = (long)entry[0];
                double open = (double)entry[1];
                double high = (double)entry[2];
                double low = (double)entry[3];
                double close = (double)entry[4];
                    closingPrices.Add((decimal)close); // for modifier 2
                double volume = (double)entry[5];

                // Estimate liquidity based on price stability and volume
                double spread = high - low;
                double relativeSpread = spread / close;
                double liquidityEstimate = volume / Math.Max(relativeSpread, 0.00001); // avoid div by zero

                // for modifier 1
                if (liquidityEstimate > maxEstimatedLiquidity) { maxEstimatedLiquidity = (double)liquidityEstimate; }
            }

            // for modifier 2
            ( decimal small, decimal big) = CalculateAveragePrice(closingPrices);

            return ( maxEstimatedLiquidity, small, big );

        }
        else
        {
            Debug.LogError("Failed to fetch: " + www.error);
            return ( 0, 0, 0 );
        }

    }



    private async Task<(int, int)> checkDecimals(Web3 web3){

        // Create contract query handlers
        var token0Handler = web3.Eth.GetContractQueryHandler<Token0Function>();
        var token1Handler = web3.Eth.GetContractQueryHandler<Token1Function>();

        // Query token0 and token1 addresses
        var token0Address = await token0Handler
            .QueryAsync<string>(SDKManager.Instance.pairContractAddress, new Token0Function());

        var token1Address = await token1Handler
            .QueryAsync<string>(SDKManager.Instance.pairContractAddress, new Token1Function());

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

    private (decimal, decimal) CalculateAveragePrice(List<decimal> closingPrices)
    {
        if (closingPrices.Count == 0) return (0, 0);

        decimal small = closingPrices[0];
        decimal big = closingPrices[0];
        
        // Debug.Log($" small: {small} big: {big}");

        foreach (decimal price in closingPrices)
        {
            if( price < small ){
                small = price;
            }

            if( price > big ){
                big = price;
            }
        }
        return ( small, big );
    }

    public (decimal, decimal, decimal) CalculateExchangeablePercentage(BigInteger totalToken)
    {
        double marketPrice = (double)SDKManager.Instance.marketPrice;
        double modifier = SDKManager.Instance.modifier1;

        // 1. Convert BigInteger to double to use in math
        double totalTokenDbl = (double)totalToken;

        // 2. Calculate floating-point logic
        double effectivePrice = marketPrice * modifier;
        double exchangeablePercentage = modifier;
        double exchangeableTokensDbl = Math.Floor(totalTokenDbl * exchangeablePercentage);
        double ethReceivedDbl = exchangeableTokensDbl * effectivePrice;
        double usdReceivedDbl = exchangeableTokensDbl * SDKManager.Instance.priceUsd;

        // 3. Convert results back to decimal
        decimal exchangeableTokens = (decimal)exchangeableTokensDbl;
        decimal ethReceived = (decimal)ethReceivedDbl;
        decimal usdReceived = (decimal)usdReceivedDbl;

        // Debug.Log($"effectivePrice: {effectivePrice} exchangeableTokensDbl: {exchangeableTokensDbl} ethReceivedDbl: {ethReceivedDbl} usdReceivedDbl: {usdReceivedDbl}");

        return (exchangeableTokens, ethReceived, usdReceived );
    }

    [FunctionOutput]
    public class Reserves : IFunctionOutputDTO
    {
        [Parameter("uint112", "reserve0", 1)]
        public BigInteger Reserve0 { get; set; }

        [Parameter("uint112", "reserve1", 2)]
        public BigInteger Reserve1 { get; set; }

        [Parameter("uint32", "blockTimestampLast", 3)]
        public uint BlockTimestampLast { get; set; }
    }

    // This will get token0 address
    [Function("token0", "address")]
    public class Token0Function : FunctionMessage { }

    // This will get token1 address
    [Function("token1", "address")]
    public class Token1Function : FunctionMessage { }

    [Function("decimals", "uint8")]
    public class DecimalsFunction : FunctionMessage { }

    private async Task GetPairAddress(){
          
        string factoryAddress = "0x5C69bEe701ef814a2B6a3EDD4B1652CB9cc5aA6f"; // Uniswap V2
        string factoryAbi = @"[ { 'constant': true, 'inputs': [ { 'name': 'tokenA', 'type': 'address' }, { 'name': 'tokenB', 'type': 'address' } ], 'name': 'getPair', 'outputs': [ { 'name': 'pair', 'type': 'address' } ], 'payable': false, 'stateMutability': 'view', 'type': 'function' } ]";

        var web3 = new Web3(SDKManager.Instance.testRpcUrl);
        var factory = web3.Eth.GetContract(factoryAbi, factoryAddress);
        var getPair = factory.GetFunction("getPair");

        string slp = "0xCC8Fa225D80b9c7D42F96e9570156c65D6cAAa25";
        string weth = "0xC02aaA39b223FE8D0A0e5C4F27eAD9083C756Cc2";

        string pairAddress = await getPair.CallAsync<string>(slp, weth);
        // Debug.Log("SLP/WETH pair address: " + pairAddress);

    }




}
