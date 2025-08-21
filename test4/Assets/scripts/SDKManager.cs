using UnityEngine;
using UnityEngine.SceneManagement;

using System;
using System.Threading.Tasks;
using System.Numerics;
//using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

using Nethereum.Web3;
using Nethereum.Hex.HexTypes;
using Nethereum.JsonRpc.Client;
using Nethereum.ABI.FunctionEncoding.Attributes;

using UnityEngine.Networking;

using Newtonsoft.Json.Linq;

using Nethereum.Contracts;
using Nethereum.ABI;
using Nethereum.ABI.FunctionEncoding;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.ABI.Encoders;  
using Nethereum.Util;



public class SDKManager : MonoBehaviour
{
    public static SDKManager Instance{ get; private set; }
    public Web3 Web3 { get; private set; } 
    [NonSerialized] public string rpcUrl = "http://192.168.100.60:8545"; 
    //[NonSerialized] public string rpcUrl = "http://0.0.0.0:8545"; 

    private static string csvFile = "players_balances.csv";

    [HideInInspector] public string walletAddress = "0x4aB5E0D87B8f27036a472ACAe9D7FbD34af4F51c";
    [HideInInspector] public string walletAddress2 = "0xEeC5bD1f87A918E56644DD60c09848B88Ec410e7";
    [HideInInspector] public string walletAddress3 = "0x5cdC304b37cF49A5FF9cbA6C59c8A8529F443164";
    [HideInInspector] public string walletAddress4 = "0x7BE00eD244D11b14ff184a0dA503A9A4659D3A41";
    [HideInInspector] public string walletAddress5 = "0xD4c9b0818E0FEcCa7aC2096749eDA754a9c9c857";



    public int selectedCharacterIndex = 0; // default to first character
    public BigInteger totalCoin = 0;
    public int playerId = 1;

    public double minModifier = 0.0f;
    public double maxModifier = 0.0f;
    public double midModifier = 0.0f;
    
    [HideInInspector]public double priceUsd = 0;
    [HideInInspector]public decimal marketPrice = 0;
    [HideInInspector]public double TotalExchangeableToken = 0;
    [HideInInspector]public double TotalToken = 0;

    [HideInInspector]public double modifier1 = 0;
    [HideInInspector]public double modifierOne = 0;
    [HideInInspector]public double modifierTwo = 0;
    [HideInInspector]public double modifierThree = 0;

    [HideInInspector]public double reserveValue = 0;
    [HideInInspector]public double reserve0 = 0;
    [HideInInspector]public double reserve1 = 0;

    public MerkleTree CurrentMerkleTree { get; private set; }
    public List<byte[]> CurrentLeaves { get; private set; }
    public BigInteger CurrentEpochId { get; private set; }

    async void Start()
    {

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        Web3 = new Web3(rpcUrl);
        Debug.Log("Web3 connected from SDKManager to: " + rpcUrl + " web3 " + Web3);

        List<string> Addresses = new List<string>
                {
                    walletAddress,
                    walletAddress2,
                    walletAddress3,
                    walletAddress4,
                    walletAddress5
                };


         // 🔹 Start epoch loop
        await StartEpochLoopAsync(Addresses);

    }

    private async Task StartEpochLoopAsync(List<string> playerAddresses)
    {
        var pairContract = Web3.Eth.GetContract(ABIManager.tokenpairABI, ABIManager.tokenpairAddress);
        var epochDurationFn = pairContract.GetFunction("epochDuration");

        while (true)
        {
            // 🔹 Call balances + build & push Merkle root
            await CheckAndSaveBalancesAsync(playerAddresses);

            // 🔹 Get epoch duration from contract
            BigInteger durationSec = await epochDurationFn.CallAsync<BigInteger>();
            double durationFloat = (double)durationSec;

            Debug.Log($"[SDKManager] Waiting {durationFloat} seconds until next epoch...");

            // 🔹 Wait for next epoch
            await Task.Delay(TimeSpan.FromSeconds(durationFloat));
        }
    }

    // Read balances for all addresses in the list
    public async Task CheckAndSaveBalancesAsync(List<string> playerAddresses)
    {
        var token0Contract = Web3.Eth.GetContract(ABIManager.mytokenABI, ABIManager.mytokenAddress);
        var token1Contract = Web3.Eth.GetContract(ABIManager.othertokenABI, ABIManager.othertokenAddress);

        var balanceOfToken0 = token0Contract.GetFunction("balanceOf");
        var balanceOfToken1 = token1Contract.GetFunction("balanceOf");

        // Safe allowance from pair contract
        var pairContract = Web3.Eth.GetContract(ABIManager.tokenpairABI, ABIManager.tokenpairAddress);
        var fn = pairContract.GetFunction("getMaxSwapInForImpact");
        var safeAllowanceHex = await fn.CallAsync<BigInteger>(true);
        var safeAllowance = Web3.Convert.FromWei(safeAllowanceHex).ToString();

        Dictionary<string, (string, string)> currentData = LoadCsv();
        List<(string address, string bal0, string bal1, string allowance)> playerInfos = new();

        foreach (var address in playerAddresses)
        {
            var bal0Hex = await balanceOfToken0.CallAsync<BigInteger>(address);
            var bal1Hex = await balanceOfToken1.CallAsync<BigInteger>(address);

            var bal0 = Web3.Convert.FromWei(bal0Hex).ToString();
            var bal1 = Web3.Convert.FromWei(bal1Hex).ToString();

            playerInfos.Add((address, bal0, bal1, safeAllowanceHex.ToString()));

            if (!currentData.ContainsKey(address)) currentData[address] = (bal0, bal1);
            else currentData[address] = (bal0, bal1);
        }

        SaveCsv(currentData);

        // 🔹 Fetch epochDuration & compute current epochId
        var epochIdFn = pairContract.GetFunction("currentEpochId");
        var epochId = await epochIdFn.CallAsync<BigInteger>();
        Debug.Log($" Current EpochId: {epochId}");

        // 🔹 Build Merkle Tree (epochId + address + allowance)
        var root = BuildMerkleTree( playerInfos, epochId );


        // 🔹 Call setMerkleRoot(epochId, root)
        var rootBytes32 = ("0x" + root).HexToByteArray();
        var setMerkleRootFn = pairContract.GetFunction("setMerkleRoot");
        var txReceipt = await setMerkleRootFn.SendTransactionAndWaitForReceiptAsync(
            from: walletAddress,
            gas: new Nethereum.Hex.HexTypes.HexBigInteger(600000),
            value: null,
            functionInput: new object[] { epochId, rootBytes32  }
        );

        Debug.Log("setMerkleRoot TxHash: " + txReceipt.TransactionHash);
    }

    public string BuildMerkleTree( List<(string address, string bal0, string bal1, string allowance)> playerInfos, BigInteger epochId  ){

        var leaves = playerInfos.Select(p =>
        {
            Debug.Log($" allowence: { p.allowance }, address: { p.address } ");
            BigInteger allowanceWei = BigInteger.Parse(p.allowance);
        
            var encoder = new ABIEncode();
            var encoded = encoder.GetABIEncodedPacked(
                new ABIValue("uint256", epochId),
                new ABIValue("address", p.address),
                new ABIValue("uint256", allowanceWei)
            );

            var leaf = Sha3Keccack.Current.CalculateHash(encoded);
            return leaf;
        }).ToList();

        var tree = new MerkleTree(leaves);

        Debug.Log($" tree: {tree} ");
        Debug.Log($" leaves: {leaves} ");
        Debug.Log($" epochId: {epochId} ");

        CurrentMerkleTree = tree;
        CurrentLeaves = leaves;
        CurrentEpochId = epochId;

        string root = BitConverter.ToString(tree.GetRoot()).Replace("-", "");
        Debug.Log("Merkle Root: " + root);
        return root;

    }

    // Load CSV
    private Dictionary<string, (string, string)> LoadCsv()
    {
        Dictionary<string, (string, string)> data = new();
        if (!File.Exists(csvFile)) return data;

        foreach (var line in File.ReadAllLines(csvFile))
        {
            var parts = line.Split(',');
            if (parts.Length == 3) data[parts[0]] = (parts[1], parts[2]);
        }
        return data;
    }

    // Save CSV
    private void SaveCsv(Dictionary<string, (string, string)> data)
    {
        using var writer = new StreamWriter(csvFile);
        foreach (var kv in data) writer.WriteLine($"{kv.Key},{kv.Value.Item1},{kv.Value.Item2}");
    }
    }