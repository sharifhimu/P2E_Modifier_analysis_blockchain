using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

using System;
using System.Numerics;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

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

    async void Start()
    {
        // Debug.Log("called");
         var ( modifier1, marketPrice, liquidity )  = await CalculateModifier12();
         SDKManager.Instance.marketPrice = marketPrice;
         
         // SDKManager.Instance.modifierOne = modifier1;
         // SDKManager.Instance.modifierTwo = modifier2;
         // SDKManager.Instance.modifierThree = modifier3;

         SDKManager.Instance.modifier1 = modifier1;
         double effectivePrice = (double)marketPrice*modifier1;
         // Debug.Log($"modifier: {modifierVal}");

        Debug.Log($" marketprice: {marketPrice}, effectivePrice: {effectivePrice} ");


         string modifierPath = Application.persistentDataPath + "/modifier1_log.csv";
         SaveModifier1ToFile(modifierPath, modifier1, liquidity, marketPrice, effectivePrice, DateTime.Now);
    }

    void Update()
    {
        
    }




    private async Task<(double, decimal, double)> CalculateModifier12()
    {
        var (reserve0, reserve1, maxLiquidity, reserveValue) = await CalculationsFromApiData();
        double liquidity = reserve0;

        double minModifier = SDKManager.Instance.minModifier;
        double midModifier = SDKManager.Instance.midModifier;
        double maxModifier = SDKManager.Instance.maxModifier;

        decimal marketPrice = (decimal)(reserve1 / reserve0);

        if (reserve0 < reserve1)
        {   
            liquidity = reserve1;
            marketPrice = (decimal)(reserve0 / reserve1);
        }

        double midLiquidity = maxLiquidity * 0.5;
        double modifier1Calc;
        double t;

        if (liquidity <= midLiquidity)
        {
            // Scale between min → mid
            t = liquidity / midLiquidity;
            modifier1Calc = minModifier + t * (midModifier - minModifier);
        }
        else
        {
            // Scale between mid → max
            t = (liquidity - midLiquidity) / (maxLiquidity - midLiquidity);
            modifier1Calc = midModifier + t * (maxModifier - midModifier);
        }

        double modifier1 = Math.Clamp(modifier1Calc, minModifier, maxModifier);

        Debug.Log($"liquidity: {liquidity}, maxLiquidity: {maxLiquidity}, reserve0: {reserve0}, reserve1: {reserve1}");
        Debug.Log($"t: {t}, midLiquidty: {midLiquidity}, liquidity <= midLiquidity : { liquidity <= midLiquidity }");
        Debug.Log($"modifier1Calc: {modifier1Calc}, modifier1: {modifier1}");

        return (modifier1, marketPrice, liquidity );
    }



    private async Task<( double, double, double, double )> CalculationsFromApiData()
    {

        double reserveValue = SDKManager.Instance.reserveValue;
        double reserve0 = SDKManager.Instance.reserve0;
        double reserve1 = SDKManager.Instance.reserve1;

        ( double dayLiquidity, decimal smallDayPrice, decimal bigDayPrice ) = await OHLCVData("day");
        ( double hourLiquidity, decimal smallHourPrice, decimal bigHourPrice ) = await OHLCVData("hour");
        ( double minuteLiquidity, decimal smallMinPrice, decimal bigMinPrice ) = await OHLCVData("minute");

        double baseOHLCV = (dayLiquidity * 0.5) + (hourLiquidity * 0.3) + (minuteLiquidity * 0.2);
        double deviation = Math.Abs(reserve0 - baseOHLCV) / reserve0;

        double dayWeight, hourWeight, minuteWeight;

        if (deviation < 0.1) // Less than 10% off → stable market
        {
            dayWeight = 0.5;
            hourWeight = 0.3;
            minuteWeight = 0.2;
        }
        else if (deviation < 0.3) // Mild deviation → rebalance
        {
            dayWeight = 0.3;
            hourWeight = 0.3;
            minuteWeight = 0.4;
        }
        else // High deviation → trust real-time data more
        {
            dayWeight = 0.2;
            hourWeight = 0.3;
            minuteWeight = 0.5;
        }

        double finalOHLCVMaxLiquidity =
            (dayLiquidity * dayWeight) +
            (hourLiquidity * hourWeight) +
            (minuteLiquidity * minuteWeight);
        finalMaxLiquidity = Math.Max(finalOHLCVMaxLiquidity, reserveValue);
        //


        Debug.Log($" weight: day: {dayWeight}, hour: {hourWeight}, minute: {minuteWeight} ");
        Debug.Log($"daily liquidity: {dayLiquidity}, hourly liquidity: {hourLiquidity}, minute-based liquidity: {minuteLiquidity}");
        Debug.Log($"daily price : smaller: {smallDayPrice}, bigger: {bigDayPrice}");
        Debug.Log($"hourly price : smaller: {smallHourPrice}, bigger: {bigHourPrice}");
        Debug.Log($"minute-based price : smaller: {smallMinPrice}, bigger: {bigMinPrice}");
        
        // Debug.Log($"smallDayPrice: {smallDayPrice} bigDayPrice: {bigDayPrice} smallHourPrice: {smallHourPrice} bigHourPrice: {bigHourPrice} smallMinPrice: {smallMinPrice} bigMinPrice: {bigMinPrice} ");

        //
        return ( reserve0, reserve1, finalMaxLiquidity, reserveValue ); 
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


    public void SaveModifier1ToFile(string filePath, double modifier1, double liquidity, decimal marketPrice, double effectivePrice, DateTime timestamp)
    {

        Debug.Log($" price market: {marketPrice}, effective: {effectivePrice} ");
        // Ensure the directory exists
        Directory.CreateDirectory(Path.GetDirectoryName(filePath));

        // Skip saving if already saved today
        if (FileChecker.AlreadySavedToday(filePath))
        {
            Debug.Log("Clamp already saved today. Skipping save.");
            return;
        }

        // If file doesn't exist, write header first
        if (!File.Exists(filePath))
        {
            string header = "Timestamp,Modifier,Liquidity,marketPrice,effectivePrice";
            File.WriteAllText(filePath, header + "\n", Encoding.UTF8);
        }

        // Format and append the modifier data
        string row = string.Format(
            "{0},{1:F6},{2:F4},{3:G},{4:G}",
            timestamp.ToString("yyyy-MM-dd HH:mm:ss"),
            modifier1,
            liquidity,
            marketPrice,
            effectivePrice
        );

        File.AppendAllText(filePath, row + "\n", Encoding.UTF8);
        Debug.Log(" Modifier1 saved: " + row);
    }



}
