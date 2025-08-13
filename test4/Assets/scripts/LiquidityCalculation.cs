using UnityEngine;
using System;
using System.Numerics;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Text;

public class LiquidityCalculation : MonoBehaviour
{
    private double finalMaxLiquidity = 0;

    async void Start()
    {
        var (modifier1, marketPrice, liquidity) = await CalculateModifierFromOnChain();
        
        SDKManager.Instance.marketPrice = marketPrice;
        SDKManager.Instance.modifier1 = modifier1;

        double effectivePrice = (double)marketPrice * modifier1;

        Debug.Log($"[OnChain] Market Price: {marketPrice}, Effective Price: {effectivePrice}");

        string modifierPath = Application.persistentDataPath + "/modifier1_log.csv";
        SaveModifier1ToFile(modifierPath, modifier1, liquidity, marketPrice, effectivePrice, DateTime.Now);
    }

    private async Task<(double, decimal, double)> CalculateModifierFromOnChain()
    {
        // সরাসরি SDKManager থেকে লোকাল ব্লকচেইনের ডাটা নিচ্ছি
        double reserve0 = SDKManager.Instance.reserve0;
        double reserve1 = SDKManager.Instance.reserve1;
        double reserveValue = SDKManager.Instance.reserveValue; // liquidity (sqrt(res0*res1))

        double minModifier = SDKManager.Instance.minModifier;
        double midModifier = SDKManager.Instance.midModifier;
        double maxModifier = SDKManager.Instance.maxModifier;

        // On-chain market price
        decimal marketPrice = (decimal)(reserve1 / reserve0);
        double liquidity = reserve0;

        if (reserve0 < reserve1)
        {
            liquidity = reserve1;
            marketPrice = (decimal)(reserve0 / reserve1);
        }

        // লোকাল ব্লকচেইনে clamp range অনুযায়ী max liquidity
        double maxLiquidity = Math.Max(liquidity, reserveValue);
        finalMaxLiquidity = maxLiquidity;

        double midLiquidity = maxLiquidity * 0.5;
        double t;
        double modifier1Calc;

        if (liquidity <= midLiquidity)
        {
            t = liquidity / midLiquidity;
            modifier1Calc = minModifier + t * (midModifier - minModifier);
        }
        else
        {
            t = (liquidity - midLiquidity) / (maxLiquidity - midLiquidity);
            modifier1Calc = midModifier + t * (maxModifier - midModifier);
        }

        double modifier1 = Math.Clamp(modifier1Calc, minModifier, maxModifier);

        Debug.Log($"[OnChain] liquidity: {liquidity}, reserve0: {reserve0}, reserve1: {reserve1}, maxLiquidity: {maxLiquidity}");
        Debug.Log($"[OnChain] t: {t}, midLiquidity: {midLiquidity}, modifier1Calc: {modifier1Calc}, modifier1: {modifier1}");

        return (modifier1, marketPrice, liquidity);
    }

    public void SaveModifier1ToFile(string filePath, double modifier1, double liquidity, decimal marketPrice, double effectivePrice, DateTime timestamp)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(filePath));

        if (FileChecker.AlreadySavedToday(filePath))
        {
            Debug.Log("Modifier already saved today. Skipping save.");
            return;
        }

        if (!File.Exists(filePath))
        {
            string header = "Timestamp,Modifier,Liquidity,MarketPrice,EffectivePrice";
            File.WriteAllText(filePath, header + "\n", Encoding.UTF8);
        }

        string row = string.Format(
            "{0},{1:F6},{2:F4},{3:G},{4:G}",
            timestamp.ToString("yyyy-MM-dd HH:mm:ss"),
            modifier1,
            liquidity,
            marketPrice,
            effectivePrice
        );

        File.AppendAllText(filePath, row + "\n", Encoding.UTF8);
        Debug.Log("Modifier1 saved: " + row);
    }
}
