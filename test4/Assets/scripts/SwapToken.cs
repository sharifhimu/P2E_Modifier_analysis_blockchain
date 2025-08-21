using UnityEngine;
using UnityEngine.UI;
using Nethereum.Web3;
using Nethereum.ABI.FunctionEncoding.Attributes;
using System.Numerics;
using System.Threading.Tasks;
using TMPro;
using Nethereum.Hex.HexTypes;
using UnityEngine.SceneManagement;
using System.IO;
using System;

using Nethereum.ABI;  
using Nethereum.ABI.Encoders;  
using Nethereum.Util;
using System.Collections.Generic;
using System.Linq;


public class SwapToken : MonoBehaviour
{
    
    public TMP_Dropdown tokenSelection;
    public TMP_InputField amountInput;
    public TMP_Text exchangeRateText;
    public TMP_Text estimateOutput;

    public TMP_Text warningText;
    public decimal maxSwapAllowed;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void Start()
    {
        var web3 = SDKManager.Instance.Web3;
        var pairContract = web3.Eth.GetContract(ABIManager.tokenpairABI, ABIManager.tokenpairAddress);
        var fn = pairContract.GetFunction("getMaxSwapInForImpact");
        var maxIn = await fn.CallAsync<BigInteger>(true);
        var maxInBig = Web3.Convert.FromWei(maxIn);
        Debug.Log($"maxIn: {maxInBig}");      

        decimal maxSwapDecimal = (decimal)maxInBig; 
        BigInteger maxSwapWei = new BigInteger(maxSwapDecimal * (decimal)Math.Pow(10, 18));

        maxSwapAllowed = (decimal)maxSwapWei;

        amountInput.onValueChanged.AddListener(OnInputChanged);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnInputChanged(string input)
    {
        if (string.IsNullOrEmpty(input)) return;

        if (decimal.TryParse(input, out decimal value))
        {
            if (value > maxSwapAllowed)
            {
                // Enforce max value
                amountInput.text = "0";

                // Show warning
                if (warningText != null)
                    warningText.text = $"⚠️ You can only swap up to {maxSwapAllowed} tokens this time";
            }
            else
            {
                if (warningText != null)
                    warningText.text = ""; // clear warning
            }
        }
        else
        {
            // Invalid input (not a number) → clear field
            amountInput.text = "";
            if (warningText != null)
                warningText.text = "Please enter a valid number.";
        }
    }

    public void Back(){
        SceneManager.LoadScene("Scene2");
    }

    public async void checkExchngRate(){
        var web3 = SDKManager.Instance.Web3;

         // Get reserves
        var pairContract = web3.Eth.GetContract(ABIManager.tokenpairABI, ABIManager.tokenpairAddress);
        var reserves = await pairContract.GetFunction("getReserves").CallDeserializingToObjectAsync<Reserves>();

        // Load token contracts
        var token0Contract = web3.Eth.GetContract(ABIManager.mytokenABI, ABIManager.mytokenAddress);
        var token1Contract = web3.Eth.GetContract(ABIManager.othertokenABI, ABIManager.othertokenAddress);

        // Get decimals
        int decimals0 = await token0Contract.GetFunction("decimals").CallAsync<int>();
        int decimals1 = await token1Contract.GetFunction("decimals").CallAsync<int>();

        // Scale to token units
        BigInteger normalizedReserve0 = reserves.Reserve0 / BigInteger.Pow(10, decimals0);
        BigInteger normalizedReserve1 = reserves.Reserve1 / BigInteger.Pow(10, decimals1);

        decimal price;
        if (tokenSelection.value == 0) // Token0 → Token1
        {
            price = (decimal)normalizedReserve1 / (decimal)normalizedReserve0;
            exchangeRateText.text = $"1 Token0 = {price} Token1";
        }
        else // Token1 → Token0
        {
            price = (decimal)normalizedReserve0 / (decimal)normalizedReserve1;
            exchangeRateText.text = $"1 Token1 = {price} Token0";
        }

    }


   public async void EstimateSwap()
    {

        var web3 = SDKManager.Instance.Web3;

         // Get reserves
        var pairContract = web3.Eth.GetContract(ABIManager.tokenpairABI, ABIManager.tokenpairAddress);
        var reserves = await pairContract.GetFunction("getReserves").CallDeserializingToObjectAsync<Reserves>();

        // Load token contracts
        var token0Contract = web3.Eth.GetContract(ABIManager.mytokenABI, ABIManager.mytokenAddress);
        var token1Contract = web3.Eth.GetContract(ABIManager.othertokenABI, ABIManager.othertokenAddress);

        // Get decimals
        int decimals0 = await token0Contract.GetFunction("decimals").CallAsync<int>();
        int decimals1 = await token1Contract.GetFunction("decimals").CallAsync<int>();

        // Scale to token units
        BigInteger normalizedReserve0 = reserves.Reserve0; // / BigInteger.Pow(10, decimals0);
        BigInteger normalizedReserve1 = reserves.Reserve1; // / BigInteger.Pow(10, decimals1);

        decimal amount = decimal.Parse(amountInput.text);
        BigInteger amountInWei = Web3.Convert.ToWei(amount);

        BigInteger reserveIn = (tokenSelection.value == 0) ? normalizedReserve0 : normalizedReserve1;
        BigInteger reserveOut = (tokenSelection.value == 0) ? normalizedReserve1 : normalizedReserve0;

        BigInteger amountInWithFee = amountInWei * 997;
        BigInteger numerator = amountInWithFee * reserveOut;
        BigInteger denominator = (reserveIn * 1000) + amountInWithFee;
        BigInteger amountOutWei = numerator / denominator;

        decimal amountOut = Web3.Convert.FromWei(amountOutWei);
        estimateOutput.text = $"Estimated Output: {amountOut}";
    }

    public List<byte[]> GetMerkleProof()
    {
           Debug.Log($" SDKManager.Instance.CurrentMerkleTree: {SDKManager.Instance.CurrentMerkleTree} ");
           Debug.Log($" SDKManager.Instance.CurrentLeaves: {SDKManager.Instance.CurrentLeaves} ");
           Debug.Log($" SDKManager.Instance.CurrentEpochId: {SDKManager.Instance.CurrentEpochId} ");

        if  (SDKManager.Instance.CurrentMerkleTree == null || SDKManager.Instance.CurrentLeaves == null)
        {
            Debug.LogError("Merkle tree not built yet!");
            return null;
        }

        // Encode the player leaf again (same as BuildMerkleTree)
        var encoder = new ABIEncode();
        var encoded = encoder.GetABIEncodedPacked(
            new ABIValue("uint256", SDKManager.Instance.CurrentEpochId),
            new ABIValue("address", SDKManager.Instance.walletAddress),
            new ABIValue("uint256", maxSwapAllowed) // allowence
        );
        var leaf = Sha3Keccack.Current.CalculateHash(encoded);

        Debug.Log($" leaf: {leaf} ");

        // Find index of leaf
        int index = SDKManager.Instance.CurrentLeaves.FindIndex(l => l.SequenceEqual(leaf));
        if (index < 0)
        {
            Debug.LogError("Leaf not found in Merkle Tree for this player!");
            return null;
        }

        // Return proof
        var proof = SDKManager.Instance.CurrentMerkleTree.GetProof(index);
        Debug.Log($" proof: {proof} ");
        return proof;
    }


    public async void SwapTokens()
    {
        var web3 = SDKManager.Instance.Web3;

         // Step 1: Load CSV & Calculate EMA Modifiers
        // string csvPath = Path.Combine(Application.persistentDataPath, "swap_log.csv");
    
        List<byte[]> merkleProof = GetMerkleProof();
        Debug.Log($" merkle proof: {merkleProof} ");

        for (int i = 0; i < merkleProof.Count; i++)
        {
            string hex = "0x" + BitConverter.ToString(merkleProof[i]).Replace("-", "");
            Debug.Log($"Proof[{i}]: {hex}");
        }

        decimal amountDecimal = decimal.Parse(amountInput.text);
        BigInteger amountIn = Web3.Convert.ToWei(amountDecimal);
        bool isToken0To1 = (tokenSelection.value == 0);

       // Step 3: Auto-limit based on EMA results
        if (amountDecimal > (decimal)maxSwapAllowed)
        {
            
            Debug.LogWarning($"Requested swap {amountDecimal} exceeds safe max {maxSwapAllowed}. Limiting...");
            amountInput.text = maxSwapAllowed.ToString();
            Debug.Log($"Swap limited to {maxSwapAllowed} due to market volatility.");

        } else {

            // Determine tokenIn address based on dropdown selection
            string tokenInAddress;
            string tokenAbi;
            if (tokenSelection.value == 0){ 
                tokenInAddress = ABIManager.mytokenAddress;
                tokenAbi = ABIManager.mytokenABI;
            } else { 
                tokenInAddress = ABIManager.othertokenAddress;
                tokenAbi = ABIManager.othertokenABI;
            }
            string swapContractAddress = ABIManager.tokenpairAddress;

            // Approve the swap contract
            var tokenContract = web3.Eth.GetContract( tokenAbi, tokenInAddress);
            var approveFunction = tokenContract.GetFunction("approve");
            var approveGas = await approveFunction.EstimateGasAsync( SDKManager.Instance.walletAddress, null, null, swapContractAddress, amountIn );


            Debug.Log($"Approving {amountIn} tokens for {swapContractAddress}...");
            var approveTxHash = await approveFunction.SendTransactionAsync(
                SDKManager.Instance.walletAddress,
                new Nethereum.Hex.HexTypes.HexBigInteger(600000),
                null,
                swapContractAddress,
                amountIn
            );

            Debug.Log("Approve transaction hash: " + approveTxHash);

            // Optional: wait for receipt before continuing
            var receipt = await web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(approveTxHash);
            while (receipt == null)
            {
                await Task.Delay(2000);
                receipt = await web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(approveTxHash);
            }
            Debug.Log("Approve confirmed.");

            // Call swap function
            var swapContract = web3.Eth.GetContract( ABIManager.tokenpairABI, swapContractAddress);
            // var swapFunction = swapContract.GetFunction("swap");
            // var swapGas = await swapFunction.EstimateGasAsync( SDKManager.Instance.walletAddress, null, null, amountIn, isToken0To1 );
            var swapFunction = swapContract.GetFunction("swapWithProof");
            var swapGas = await swapFunction.EstimateGasAsync( SDKManager.Instance.walletAddress, null, null, amountIn, isToken0To1, maxSwapAllowed, merkleProof );
            swapGas = new HexBigInteger(swapGas.Value + (swapGas.Value / 5)); // +20% buffer


            Debug.Log("Calling swap...");
            var swapTxHash = await swapFunction.SendTransactionAsync(
                SDKManager.Instance.walletAddress,
                swapGas,
                null,
                amountIn,
                isToken0To1,
                maxSwapAllowed,
                merkleProof
            );

            Debug.Log("Swap transaction hash: " + swapTxHash);
        
            // Get reserves
            var pairContract = web3.Eth.GetContract(ABIManager.tokenpairABI, ABIManager.tokenpairAddress);
            var reserves = await pairContract.GetFunction("getReserves").CallDeserializingToObjectAsync<Reserves>();
            var normalizedReserve0 = Web3.Convert.FromWei( reserves.Reserve0 );
            var normalizedReserve1 = Web3.Convert.FromWei( reserves.Reserve1 );
            var token0price = (decimal)normalizedReserve1 / (decimal)normalizedReserve0;
            var token1price = (decimal)normalizedReserve0 / (decimal)normalizedReserve1;

            LogSwap( isToken0To1 ? amountDecimal : 0, isToken0To1 ? 0 : amountDecimal, normalizedReserve0, normalizedReserve1, token0price, token1price );
        
        }


    }

    public static void LogSwap(
        decimal swapAmountToken0,
        decimal swapAmountToken1,
        decimal reserveToken0,
        decimal reserveToken1,
        decimal marketPriceToken0,
        decimal marketPriceToken1
    )
    {
        try
        {
            string filePath = Path.Combine(Application.persistentDataPath, "swap_log.csv");
            // Create file with headers if not exists
            if (!File.Exists(filePath))
            {
                using (StreamWriter sw = File.CreateText(filePath))
                {
                    sw.WriteLine("Timestamp,SwapAmountToken0,SwapAmountToken1,ReserveToken0,ReserveToken1,MarketPriceToken0( token0 worth how much token1 ),MarketPriceToken1( token1 worth how much token0 )");
                }
            }

            // Append swap data
            using (StreamWriter sw = File.AppendText(filePath))
            {
                string timestamp = System.DateTime.UtcNow.ToString("o"); // ISO 8601 format
                string line = $"{timestamp},{swapAmountToken0},{swapAmountToken1},{reserveToken0},{reserveToken1},{marketPriceToken0},{marketPriceToken1}";
                sw.WriteLine(line);
            }

            Debug.Log($"Swap logged: {swapAmountToken0} Token0 / {swapAmountToken1} Token1");
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Error writing swap log to CSV: " + ex.Message);
        }
    }


}
