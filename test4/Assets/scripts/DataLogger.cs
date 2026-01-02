using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using UnityEngine;

[System.Serializable]
public class OperationRecord
{
    public string operationType; // "SetMerkleRoot", "AddLiquidity", "Swap"
    public string transactionHash;
    public long timestamp;
    public string player;
    public string method;
    public double gasUsed;
    public double gasPrice; // in Gwei
    public double gasCostETH;
    public string blockNumber;

    // --- NEW: swap + pool state (only for Swap) ---
    public double amountIn;          // in token units
    public double amountOut;         // in token units
    public string swapDirection;     // "0->1" or "1->0"

    public double reserve0Before;
    public double reserve1Before;
    public double reserve0After;
    public double reserve1After;
    public double kBefore;
    public double kAfter;

    public double token0Balance;
    public double token1Balance;
    public string maxSwapAllowed;
    public string epochId;
    public double executionTime;
}

[System.Serializable]
public class OperationLog
{
    public List<OperationRecord> operations = new List<OperationRecord>();
}


public class DataLogger : MonoBehaviour
{
    public static DataLogger Instance { get; private set; }

    private OperationLog operationLog;
    private string logFilePath;
    private string etherescanApiKey = "XU1J17ERN6SZ6MU6WZH7AMJ29RAA155WBW";
    private string chainId = "11155111"; // Sepolia

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
        string dataFolder = Path.Combine(projectPath, "TestnetData");

        if (!Directory.Exists(dataFolder))
        {
            Directory.CreateDirectory(dataFolder);
        }

        logFilePath = Path.Combine(dataFolder, "operations_log.json");
        LoadOrCreateLog();
    }

    private void LoadOrCreateLog()
    {
        if (File.Exists(logFilePath))
        {
            string json = File.ReadAllText(logFilePath);
            operationLog = JsonConvert.DeserializeObject<OperationLog>(json);
        }
        else
        {
            operationLog = new OperationLog();
        }
    }

    public async void LogOperation(
        string operationType,
        string txHash,
        string playerAddress,
        double amountIn = 0,
        double amountOut = 0,
        string swapDirection = "",
        double reserve0Before = 0,
        double reserve1Before = 0,
        double reserve0After = 0,
        double reserve1After = 0,
        double kBefore = 0,
        double kAfter = 0,
        double token0Balance=0, 
        double token1Balance=0,
        string maxSwapAllowed="",
        string epochId = "",
        double executionTime = 0
    )
    {
        Debug.Log($"📊 Logging {operationType}: {txHash}: {playerAddress}");

        var txDetails = await FetchTransactionDetailsAsync(txHash);

        if (txDetails != null)
        {
            double gasUsedDecimal = HexToDecimal(txDetails["gas"]);
            double gasPriceWei = HexToDecimal(txDetails["gasPrice"]);
            double gasPriceGwei = gasPriceWei / 1e9;
            double gasCostETH = (gasUsedDecimal * gasPriceWei) / 1e18;

            OperationRecord record = new OperationRecord
            {
                operationType = operationType,
                transactionHash = txHash,
                timestamp = (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds,
                player = playerAddress,
                method = DecodeFunctionName(txDetails["input"]),
                gasUsed = gasUsedDecimal,
                gasPrice = gasPriceGwei,
                gasCostETH = gasCostETH,
                blockNumber = txDetails["blockNumber"],

                // NEW
                amountIn = amountIn,
                amountOut = amountOut,
                swapDirection = swapDirection,
                reserve0Before = reserve0Before,
                reserve1Before = reserve1Before,
                reserve0After = reserve0After,
                reserve1After = reserve1After,
                kBefore = kBefore,
                kAfter = kAfter,
                token0Balance = token0Balance,
                token1Balance = token1Balance,
                maxSwapAllowed = maxSwapAllowed,
                epochId = epochId,
                executionTime = executionTime
            };

            operationLog.operations.Add(record);
            SaveLog();

            Debug.Log($"✅ Logged: {operationType} | Gas: {record.gasUsed:F0} | Price: {record.gasPrice:F6} Gwei | Cost: {record.gasCostETH:F8} ETH");
        }
    }
    private async System.Threading.Tasks.Task<Dictionary<string, string>> FetchTransactionDetailsAsync(string txHash)
    {
        string url = $"https://api.etherscan.io/v2/api?module=proxy&action=eth_getTransactionByHash&txhash={txHash}&chainid={chainId}&apikey={etherescanApiKey}";

        using (var client = new System.Net.Http.HttpClient())
        {
            try
            {
                var response = await client.GetAsync(url);
                string json = await response.Content.ReadAsStringAsync();

                Debug.Log($"📨 API Response: {json}");

                var wrapper = JsonConvert.DeserializeObject<JObject>(json);

                if (wrapper == null || wrapper["result"] == null)
                {
                    Debug.LogError($"❌ Invalid response: {json}");
                    return null;
                }

                var result = wrapper["result"];

                return new Dictionary<string, string>
                {
                    { "gas", result["gas"]?.ToString() ?? "0" },
                    { "gasPrice", result["gasPrice"]?.ToString() ?? "0" },
                    { "blockNumber", result["blockNumber"]?.ToString() ?? "0" },
                    { "input", result["input"]?.ToString() ?? "" },
                    { "from", result["from"]?.ToString() ?? "" }
                };
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"❌ Error fetching TX: {ex.Message}\n{ex.StackTrace}");
            }
        }

        return null;
    }


    private double HexToDecimal(string hexValue)
    {
        if (string.IsNullOrEmpty(hexValue)) return 0;
        return Convert.ToDouble(Convert.ToInt64(hexValue, 16));
    }

    private string DecodeFunctionName(string input)
    {
        if (input.Length < 10) return "Unknown";

        string selector = input.Substring(0, 10);

        Dictionary<string, string> functionMap = new Dictionary<string, string>
        {
            { "0x18712c21", "SetMerkleRoot" },
            { "0x5aa4e636", "AddLiquidity" },
            { "0x97419eec", "Swap" }
        };

        return functionMap.ContainsKey(selector) ? functionMap[selector] : $"Unknown_{selector}";
    }

    private void SaveLog()
    {
        string json = JsonConvert.SerializeObject(operationLog, Formatting.Indented);
        File.WriteAllText(logFilePath, json);

#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif
    }

    [ContextMenu("Export to CSV")]
    public void ExportToCSV()
    {
        string csvPath = Path.Combine(Path.GetDirectoryName(logFilePath), "operations_analysis.csv");

        using (StreamWriter writer = new StreamWriter(csvPath))
        {
            writer.WriteLine(
                "Timestamp,OperationType,Player,TxHash,GasUsed,GasPriceGwei,GasCostETH,Method," +
                "AmountIn,AmountOut,SwapDirection,Reserve0Before,Reserve1Before,token0Balance,token1Balance,maxSwapAllowed,epochId,executionTime"
            );

            foreach (var op in operationLog.operations)
            {
                writer.WriteLine(
                    $"{op.timestamp},{op.operationType},{op.player},{op.transactionHash}," +
                    $"{op.gasUsed:F0},{op.gasPrice:F6},{op.gasCostETH:F8},{op.method}," +
                    $"{op.amountIn},{op.amountOut},{op.swapDirection}," +
                    $"{op.reserve0Before},{op.reserve1Before},{op.token0Balance},{op.token1Balance},{op.maxSwapAllowed},{op.epochId},{op.executionTime}"
                );
            }
        }

        Debug.Log($"✅ CSV Exported: {csvPath}");
    }


    [ContextMenu("Generate Statistics")]
    public void GenerateStatistics()
    {
        Debug.Log("\n=== OPERATION STATISTICS ===");

        Dictionary<string, List<OperationRecord>> grouped = new Dictionary<string, List<OperationRecord>>();

        foreach (var op in operationLog.operations)
        {
            if (!grouped.ContainsKey(op.operationType))
                grouped[op.operationType] = new List<OperationRecord>();
            grouped[op.operationType].Add(op);
        }

        foreach (var group in grouped)
        {
            Debug.Log($"\n{group.Key}:");
            Debug.Log($"  Count: {group.Value.Count}");
            Debug.Log($"  Avg Gas: {GetAverage(group.Value, r => r.gasUsed):F0}");
            Debug.Log($"  Avg Price: {GetAverage(group.Value, r => r.gasPrice):F6} Gwei");
            Debug.Log($"  Total Cost: {GetSum(group.Value, r => r.gasCostETH):F8} ETH");
        }
    }

    private double GetAverage(List<OperationRecord> list, System.Func<OperationRecord, double> selector)
    {
        if (list.Count == 0) return 0;
        double sum = 0;
        foreach (var item in list) sum += selector(item);
        return sum / list.Count;
    }

    private double GetSum(List<OperationRecord> list, System.Func<OperationRecord, double> selector)
    {
        double sum = 0;
        foreach (var item in list) sum += selector(item);
        return sum;
    }
}
