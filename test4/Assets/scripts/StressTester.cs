using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class StressTester : MonoBehaviour
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
        var path = Path.Combine(Application.persistentDataPath, "modifier3_data.csv");
        // Write header
        File.WriteAllText(path, "total,exchanged,ratio,modifier3,finalModifier\n");

        for (int i = 0; i < testIterations; i++)
        {
            // 1) Random total tokens [1..1e6], random exchanged [0..total]
            double total    =  UnityEngine.Random.Range(1f, 1e6f);
            double exchanged=  UnityEngine.Random.Range(0f, (float)total);

            // 2) Compute ratio and expected modifier₃
            double ratio = exchanged / total;

            // Debug.Log($"total: {total}, exchanged: {exchanged}, ratio: {ratio}");

            // 3) Invoke the same code path you’ve written
            double modifier3  = SimulateModifier3(ratio);

            File.AppendAllText(path, $"{total:F4},{exchanged:F4},{ratio:F4},{modifier3:F4},{SDKManager.Instance.modifier1}\n");

            // 5) Yield occasionally so Unity remains responsive
            // if (i % 10 == 0) yield return null;

            yield return null;
        }
    }

    public double SimulateModifier3(double ratio)
    {
        var modifier3 = (ratio < 0.2) ? SDKManager.Instance.maxModifier : 
                            (ratio > 0.5) ? SDKManager.Instance.minModifier : SDKManager.Instance.midModifier;
        return modifier3;
    }

}
