using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class EMACalculation : MonoBehaviour
{

	public static void CalculateEMAFromCSV(string csvFilePath, int emaPeriod, float bootstrapTolerance, int minDataPoints, out float modifierMin, out float modifierMid, out float modifierMax)
    {
        List<float> liquidityHistory = new List<float>();

        if (!File.Exists(csvFilePath))
        {
            Debug.LogWarning("CSV not found for EMA calculation.");
            modifierMin = modifierMid = modifierMax = 0;
            return;
        }

        // Read CSV and extract liquidity values
        using (var reader = new StreamReader(csvFilePath))
        {
            bool isFirstRow = true;
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                if (isFirstRow) { isFirstRow = false; continue; }

                var values = line.Split(',');
                if (values.Length >= 4 &&
                    float.TryParse(values[2], out float reserve0) &&
                    float.TryParse(values[3], out float reserve1))
                {
                    liquidityHistory.Add(reserve0 * reserve1);
                }
            }
        }

        // If not enough data, bootstrap
        float emaLiquidity;
        float dynamicTolerance;
        if (liquidityHistory.Count < minDataPoints)
        {
            emaLiquidity = liquidityHistory.Count > 0 ? liquidityHistory[0] : 1f;
            dynamicTolerance = bootstrapTolerance;
        }
        else
        {
            emaLiquidity = liquidityHistory[0];
            float k = 2f / (emaPeriod + 1);
            for (int i = 1; i < liquidityHistory.Count; i++)
            {
                emaLiquidity = liquidityHistory[i] * k + emaLiquidity * (1 - k);
            }

            // Calculate dynamic tolerance from standard deviation
            float avg = 0;
            foreach (var val in liquidityHistory) avg += val;
            avg /= liquidityHistory.Count;

            float variance = 0;
            foreach (var val in liquidityHistory) variance += (val - avg) * (val - avg);
            variance /= liquidityHistory.Count;

            float stdDev = Mathf.Sqrt(variance);
            dynamicTolerance = stdDev / emaLiquidity;
        }

        // Final modifiers
        modifierMid = emaLiquidity;
        modifierMin = emaLiquidity * (1 - dynamicTolerance);
        modifierMax = emaLiquidity * (1 + dynamicTolerance);
    }

}
