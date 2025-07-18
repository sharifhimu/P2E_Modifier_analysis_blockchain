using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class StressTesterModifier1 : MonoBehaviour
{
    
    [Tooltip("How many random scenarios to run")]
    public int testIterations = 1000;


    /// <summary>
    /// Call this from your UI Button OnClick()
    /// </summary>
    public void RunStressTest()
    {
        StartCoroutine(StressTestRoutine());
    }

    private IEnumerator StressTestRoutine()
    {
        var path = Path.Combine(Application.persistentDataPath, "modifier1_data.csv");
        // Write header
        File.WriteAllText(path, "liquidity,maxLiquidity,liquidity/maxLiquidity,modifier1\n");

        for (int i = 0; i < testIterations; i++)
        {
            // Generate random liquidity and maxLiquidity
            double maxLiquidity = UnityEngine.Random.Range(1e3f, 1e6f); // Avoid 0
            double liquidity = UnityEngine.Random.Range(0f, (float)maxLiquidity);
            double ratio = liquidity / maxLiquidity;
            // Simulate your actual logic
            double actual = SimulateModifier1(ratio);

            File.AppendAllText(path, $"{liquidity:F4},{maxLiquidity:F4},{ratio:F4},{actual:F4}\n");

            if (i % 10 == 0) yield return null; // Keep Unity responsive
        }
    }

    public double SimulateModifier1(double ratio)
    {
        return Math.Clamp(ratio, SDKManager.Instance.minModifier, SDKManager.Instance.maxModifier);
    }

}
