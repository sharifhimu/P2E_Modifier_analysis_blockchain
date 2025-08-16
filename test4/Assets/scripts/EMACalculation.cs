using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Numerics;
using System.Threading.Tasks;
using Nethereum.Web3;

public class EMACalculation : MonoBehaviour
{
    public static async Task<(BigInteger, BigInteger)> GetReserveFromChain()
    {
        var web3 = SDKManager.Instance.Web3;
        var pairContract = web3.Eth.GetContract(ABIManager.tokenpairABI, ABIManager.tokenpairAddress);
        var reserves = await pairContract.GetFunction("getReserves")
                                         .CallDeserializingToObjectAsync<Reserves>();
        return (reserves.Reserve0, reserves.Reserve1);
    }

    public async static Task<(float modifierMin, float modifierMid, float modifierMax)> CalculateEMAFromCSV(
        string csvFilePath, int emaPeriod, float bootstrapTolerance, int minDataPoints)
    {
        List<float> liquidityHistory = new List<float>();

        // Step 1: Read CSV & normalize reserves
        if (File.Exists(csvFilePath))
        {
            using (var reader = new StreamReader(csvFilePath))
            {
                bool isFirstRow = true;
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (isFirstRow) { isFirstRow = false; continue; } // skip header

                    var values = line.Split(',');
                    if (values.Length >= 5 &&
                        decimal.TryParse(values[3], out decimal reserve0Raw) &&
                        decimal.TryParse(values[4], out decimal reserve1Raw))
                    {
                        // No need for FromWei if CSV is already in token units
                        double reserve0 = (double)reserve0Raw;
                        double reserve1 = (double)reserve1Raw;

                        liquidityHistory.Add((float)(reserve0 * reserve1));
                    }
                }
            }
        }

        float emaLiquidity;
        float dynamicTolerance;


        Debug.Log($" liquidityCount: {liquidityHistory.Count} ");
        // Step 2: If not enough history, fallback to live on-chain reserves
        if (liquidityHistory.Count < minDataPoints)
        {
            Debug.Log("bootstraped");
            var (reserve0BI, reserve1BI) = await GetReserveFromChain();
            float reserve0 = (float)(Web3.Convert.FromWei(reserve0BI));
            float reserve1 = (float)(Web3.Convert.FromWei(reserve1BI));

            float liveLiquidity = reserve0 * reserve1;

            if (liquidityHistory.Count > 0)
            {
                float historyAvg = 0f;
                foreach (var val in liquidityHistory) historyAvg += val;
                historyAvg /= liquidityHistory.Count;

                emaLiquidity = (liveLiquidity + historyAvg) / 2f;
            }
            else
            {
                emaLiquidity = liveLiquidity;
            }

            // Wider tolerance if few points
            float widenFactor = Mathf.Max(1f, (float)minDataPoints / Mathf.Max(1, liquidityHistory.Count));
            dynamicTolerance = bootstrapTolerance * widenFactor;
        }
        else
        {
            Debug.Log("EMA Calculation");
            // Step 3: Standard EMA calculation
            emaLiquidity = liquidityHistory[0];
            float k = 2f / (emaPeriod + 1);

            for (int i = 1; i < liquidityHistory.Count; i++)
                emaLiquidity = liquidityHistory[i] * k + emaLiquidity * (1 - k);

            // Step 4: Calculate tolerance from standard deviation
            float avg = 0f;
            foreach (var val in liquidityHistory) avg += val;
            avg /= liquidityHistory.Count;

            float variance = 0f;
            foreach (var val in liquidityHistory) variance += (val - avg) * (val - avg);
            variance /= liquidityHistory.Count;

            float stdDev = Mathf.Sqrt(variance);
            dynamicTolerance = stdDev / Math.Max(emaLiquidity, 0.000001f);
        }

        // Step 5: Safety checks
        if (float.IsInfinity(emaLiquidity) || float.IsNaN(emaLiquidity))
            emaLiquidity = 0f;

        // Step 6: Calculate modifiers
        var modifierMid = emaLiquidity;
        var modifierMin = emaLiquidity * (1 - dynamicTolerance);
        var modifierMax = emaLiquidity * (1 + dynamicTolerance);

        // Step 7: Clamp to avoid absurd ranges
        modifierMin = Math.Clamp(modifierMin, 0, 1_000_000_000f);
        modifierMid = Math.Clamp(modifierMid, 0, 1_000_000_000f);
        modifierMax = Math.Clamp(modifierMax, 0, 1_000_000_000f);

        Debug.Log($"[EMA] Mid={modifierMid}, Min={modifierMin}, Max={modifierMax}, Tol={dynamicTolerance}");

        return (modifierMin, modifierMid, modifierMax);
    }
}
