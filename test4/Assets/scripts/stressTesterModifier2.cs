using UnityEngine;
using System.Collections;
using System.IO;
using System.Text;
using System;

public class Modifier2StressTest : MonoBehaviour
{
    public int testIterations = 1000;
    private string filePath;

    public void RunStressTest()
    {
        StartCoroutine(StressTestRoutine());
    }

    private IEnumerator StressTestRoutine()
    {
        var path = Path.Combine(Application.persistentDataPath, "modifier2_data.csv");

        File.WriteAllText(path, "reserve0,reserve1,marketPrice,smallDay,smallHour,smallMin,bigDay,bigHour,bigMin,smallAvgPrice,bigAvgPrice,modifier2\n");

        for (int i = 0; i < testIterations; i++)
        {
            // Simulate on‐chain AMM reserves
            double reserve0 = UnityEngine.Random.Range(0.01f, 2.5f);       // Token amount (SLP or game coin)
            double reserve1 = UnityEngine.Random.Range(10000f, 500000f);   // Game token
            double marketPrice = reserve0 / reserve1;

            bool forceHigh = UnityEngine.Random.value < 0.33f;
            bool forceLow = !forceHigh && UnityEngine.Random.value < 0.5f;

            // Optional: increase variety of cases
            float deviation = UnityEngine.Random.Range(0.1f, 0.5f); // up to ±50%

            double smallDay, smallHour, smallMin;
            double bigDay, bigHour, bigMin;

            if (forceLow)
            {
                // Force marketPrice < P_low
                smallDay = marketPrice * (1.2f + deviation);
                smallHour = marketPrice * (1.1f + deviation);
                smallMin = marketPrice * (1.15f + deviation);
                bigDay = marketPrice * (1.5f + deviation);
                bigHour = marketPrice * (1.4f + deviation);
                bigMin = marketPrice * (1.45f + deviation);
            }
            else if (forceHigh)
            {
                // Force marketPrice > P_high
                smallDay = marketPrice * (0.4f - deviation);
                smallHour = marketPrice * (0.5f - deviation);
                smallMin = marketPrice * (0.45f - deviation);
                bigDay = marketPrice * (0.7f - deviation);
                bigHour = marketPrice * (0.65f - deviation);
                bigMin = marketPrice * (0.6f - deviation);
            }
            else
            {
                // Normal case
                smallDay = marketPrice * UnityEngine.Random.Range(0.7f, 0.85f);
                smallHour = marketPrice * UnityEngine.Random.Range(0.75f, 0.9f);
                smallMin = marketPrice * UnityEngine.Random.Range(0.8f, 0.92f);

                bigDay = marketPrice * UnityEngine.Random.Range(1.1f, 1.3f);
                bigHour = marketPrice * UnityEngine.Random.Range(1.12f, 1.25f);
                bigMin = marketPrice * UnityEngine.Random.Range(1.15f, 1.28f);
            }


            double P_low  = Math.Min(smallDay, Math.Min(smallHour, smallMin)); // Lower bound
            double P_high = Math.Max(bigDay, Math.Max(bigHour, bigMin));       // Upper bound

            double modifier2;
            if (marketPrice < P_low)
                modifier2 = SDKManager.Instance.maxModifier;
            else if (marketPrice > P_high)
                modifier2 = SDKManager.Instance.minModifier;
            else
                modifier2 = SDKManager.Instance.midModifier;

            File.AppendAllText(path, $"{reserve0},{reserve1},{marketPrice},{smallDay},{smallHour},{smallMin},{bigDay},{bigHour},{bigMin},{P_low},{P_high},{modifier2}\n");

            if (i % 10 == 0) yield return null; // Keep Unity responsive
        }

        Debug.Log($"Modifier2 stress test complete. Data saved to {path}");
    }
}
