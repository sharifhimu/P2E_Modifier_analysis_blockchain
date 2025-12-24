using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class UniswapSwapRecord
{
    public string transactionHash;
    public long timestamp;
    public string playerAddress;

    public double amountIn;
    public double amountOut;
    public string tokenIn;
    public string tokenOut;
    public string swapDirection;     // "0->1" or "1->0"

    // ONLY store reserves BEFORE - not after
    public double reserve0;
    public double reserve1;

    public double gasUsed;
    public double gasPriceGwei;
    public double gasCostETH;
    public double executionTime;
}

[System.Serializable]
public class UniswapSwapLog
{
    public List<UniswapSwapRecord> swaps = new List<UniswapSwapRecord>();
}

public class UniswapLogger : MonoBehaviour
{
    public static UniswapLogger Instance { get; private set; }

    private UniswapSwapLog swapLog;
    private string logFilePath;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        string projectPath = Application.dataPath;
        string dataFolder = Path.Combine(projectPath, "UniswapData");

        if (!Directory.Exists(dataFolder))
        {
            Directory.CreateDirectory(dataFolder);
        }

        logFilePath = Path.Combine(dataFolder, "uniswap_swaps.json");
        LoadOrCreateLog();
    }

    private void LoadOrCreateLog()
    {
        if (File.Exists(logFilePath))
        {
            string json = File.ReadAllText(logFilePath);
            swapLog = JsonConvert.DeserializeObject<UniswapSwapLog>(json);
        }
        else
        {
            swapLog = new UniswapSwapLog();
        }
    }

    public void LogSwap(
        string txHash,
        string playerAddress,
        double amountIn,
        double amountOut,
        string tokenIn,
        string tokenOut,
        string swapDirection,
        double reserve0,           
        double reserve1,           
        double gasUsed,
        double gasPriceGwei,
        double executionTime
    )
    {
        try
        {
            double gasCostETH = (gasUsed * gasPriceGwei) / 1e9;

            UniswapSwapRecord record = new UniswapSwapRecord
            {
                transactionHash = txHash,
                timestamp = (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds,
                playerAddress = playerAddress,

                amountIn = amountIn,
                amountOut = amountOut,
                tokenIn = tokenIn,
                tokenOut = tokenOut,
                swapDirection = swapDirection,

                reserve0 = reserve0,
                reserve1 = reserve1,

                gasUsed = gasUsed,
                gasPriceGwei = gasPriceGwei,
                gasCostETH = gasCostETH,
                executionTime = executionTime
            };

            swapLog.swaps.Add(record);
            SaveLog();

            Debug.Log($"✅ Swap Logged: {amountIn:F8} -> {amountOut:F8} | Gas: {gasCostETH:F8} ETH");
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ Error logging swap: {ex.Message}");
        }
    }

    private void SaveLog()
    {
        string json = JsonConvert.SerializeObject(swapLog, Formatting.Indented);
        File.WriteAllText(logFilePath, json);

#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif
    }

    [ContextMenu("Export to CSV")]
    public void ExportToCSV()
    {
        string csvPath = Path.Combine(Path.GetDirectoryName(logFilePath), "uniswap_swaps.csv");

        using (StreamWriter writer = new StreamWriter(csvPath))
        {
            writer.WriteLine(
                "Timestamp,TxHash,Player,AmountIn,AmountOut,TokenIn,TokenOut,Direction," +
                "Reserve0,Reserve1,GasUsed,GasPriceGwei,GasCostETH,ExecutionTime"
            );

            foreach (var swap in swapLog.swaps)
            {
                writer.WriteLine(
                    $"{swap.timestamp}," +
                    $"{swap.transactionHash}," +
                    $"{swap.playerAddress}," +
                    $"{swap.amountIn:F8}," +
                    $"{swap.amountOut:F8}," +
                    $"{swap.tokenIn}," +
                    $"{swap.tokenOut}," +
                    $"{swap.swapDirection}," +
                    $"{swap.reserve0:F8}," +
                    $"{swap.reserve1:F8}," +
                    $"{swap.gasUsed:F0}," +
                    $"{swap.gasPriceGwei:F9}," +
                    $"{swap.gasCostETH:F8}," +
                    $"{swap.executionTime:F2}"
                );
            }
        }

        Debug.Log($"✅ CSV Exported: {csvPath}");
    }
}
