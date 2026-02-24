using Nethereum.ABI;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Util;
using Nethereum.Web3;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SwapTokenUniswap : MonoBehaviour
{
    public TMP_Dropdown tokenSelection;
    public TMP_InputField amountInput;
    public TMP_Text estimateOutput;
    public TMP_Text warningText;

    private string UNISWAP_ROUTER_ADDRESS = ABIManager.uniswapRouterAddress;
    private string UNISWAP_TOKENPAIR_ADDRESS = ABIManager.uniswapPairAddress;
    private string MYT_TOKEN = ABIManager.uniswapPairMYT; // mytokenAddress
    private string OTK_TOKEN = ABIManager.uniswapPairOTK; // othertokenAddress
    private string ROUTER_ABI = ABIManager.uniswapV2ABI;
    private string ROUTER_TOKENPAIR_ABI = ABIManager.uniswapV2TokenPairABI;

    async void Start()
    {
        await CheckBalance();
    }

    public async void SwapTokens()
    {
        try
        {
            var web3 = SDKManager.Instance.Web3;
            var walletAddress = SDKManager.Instance.walletAddress;

            // ✅ FIX 1: Get reserves ONCE at the beginning
            var resultBefore = await GetReserves(web3, UNISWAP_TOKENPAIR_ADDRESS, ROUTER_TOKENPAIR_ABI);
            double reserve0 = (double)Web3.Convert.FromWei(resultBefore.Item1, 18);
            double reserve1 = (double)Web3.Convert.FromWei(resultBefore.Item2, 18);

            Debug.Log($"Reserve 0: {reserve0}");
            Debug.Log($"Reserve 1: {reserve1}");

            // Parse input amount
            decimal amountDecimal = decimal.Parse(amountInput.text);
            BigInteger amountIn = Web3.Convert.ToWei(amountDecimal, 18);

            // Determine token direction
            string tokenInAddress = (tokenSelection.value == 0) ? MYT_TOKEN : OTK_TOKEN;
            string tokenOutAddress = (tokenSelection.value == 0) ? OTK_TOKEN : MYT_TOKEN;
            string swapDirection = (tokenSelection.value == 0) ? "0->1" : "1->0";

            decimal price;
            if (tokenSelection.value == 0) // Token0 → Token1
            {
                price = (decimal)reserve0 / (decimal)reserve1;
                warningText.text = $"1 Token1 = {price} Token0";
            }
            else // Token1 → Token0
            {
                price = (decimal)reserve1 / (decimal)reserve0;
                warningText.text = $"1 Token0 = {price} Token1";
            }

            string tokenInAbi = (tokenSelection.value == 0) ? ABIManager.mytokenABI : ABIManager.othertokenABI;
            string tokenOutAbi = (tokenSelection.value == 0) ? ABIManager.othertokenABI : ABIManager.mytokenABI;

            Debug.Log($"Swapping {amountDecimal} from {tokenInAddress} to {tokenOutAddress}");

            // Step 1: APPROVE
            await ApproveToken(web3, walletAddress, tokenInAddress, tokenInAbi, amountIn);

            // Step 2: GET QUOTE
            var amountOut = await GetAmountOut(web3, amountIn, tokenInAddress, tokenOutAddress);
            double amountOutDecimal = (double)Web3.Convert.FromWei(amountOut, 18);
            BigInteger amountOutMin = (amountOut * 95) / 100;

            Debug.Log($"Expected output: {amountOutDecimal} tokens");
            Debug.Log($"Minimum output (with 5% slippage): {Web3.Convert.FromWei(amountOutMin, 18)} tokens");

            estimateOutput.text = $"{amountOutDecimal}";

            // ✅ FIX 2: Capture execution time properly
            DateTime startTime = DateTime.UtcNow;
            string swapTxHash = await ExecuteSwap(web3, walletAddress, amountIn, amountOutMin, tokenInAddress, tokenOutAddress);
            double executionTime = (DateTime.UtcNow - startTime).TotalSeconds;

            // ✅ FIX 3: Check if swap was successful before logging
            if (string.IsNullOrEmpty(swapTxHash))
            {
                Debug.LogError("Swap transaction failed - no hash returned");
                return;
            }

            // Get gas info from receipt
            var receipt = await web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(swapTxHash);

            if (receipt == null)
            {
                Debug.LogError("Could not get receipt for swap transaction");
                return;
            }

            double gasUsed = double.Parse(receipt.GasUsed.Value.ToString() ?? "0");
            double gasPriceWei = double.Parse((receipt.EffectiveGasPrice?.Value ?? 0).ToString());
            double gasPriceGwei = gasPriceWei / 1e9;

            // ✅ LOG TO UNISWAP LOGGER
            UniswapLogger.Instance.LogSwap(
                txHash: swapTxHash,
                playerAddress: walletAddress,
                amountIn: (double)amountDecimal,
                amountOut: amountOutDecimal,
                tokenIn: tokenInAddress,
                tokenOut: tokenOutAddress,
                swapDirection: swapDirection,
                reserve0: reserve0,
                reserve1: reserve1,
                gasUsed: gasUsed,
                gasPriceGwei: gasPriceGwei,
                executionTime: executionTime
            );
        }
        catch (Exception ex)
        {
            warningText.text = $"Error in swapping";
            Debug.LogError($"Swap failed: {ex}");
        }
    }

    private async Task ApproveToken(Web3 web3, string walletAddress, string tokenAddress, string tokenAbi, BigInteger amount)
    {
        try
        {
            Debug.Log($"Approving {amount} tokens for Uniswap Router...");

            var tokenContract = web3.Eth.GetContract(tokenAbi, tokenAddress);

            var approveFunction = tokenContract.GetFunction("approve");
            var gas = await approveFunction.EstimateGasAsync(
                walletAddress,
                null,                 
                null,                 
                UNISWAP_ROUTER_ADDRESS,
                amount
            );
            var gasWithBuffer = new HexBigInteger(gas.Value + gas.Value / 5);   // +20%

            var gasPrice = await web3.Eth.GasPrice.SendRequestAsync();
            var gasPriceWithBuffer = new HexBigInteger(gasPrice.Value + gasPrice.Value / 10); // +10%

            Debug.Log($" gas: {gasWithBuffer}, gasprice: {gasPriceWithBuffer} ");

            var approveTxHash = await approveFunction.SendTransactionAsync(
                walletAddress,
                gasWithBuffer,        // gas amount
                gasPriceWithBuffer,   // gas price
                null,
                UNISWAP_ROUTER_ADDRESS,
                amount
            );

            Debug.Log($"Approve tx sent: {approveTxHash}");

            var receipt = await web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(approveTxHash);
            int maxWaitAttempts = 30;
            int attempts = 0;

            while (receipt == null && attempts < maxWaitAttempts)
            {
                await Task.Delay(2000);
                receipt = await web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(approveTxHash);
                attempts++;
                Debug.Log($"Waiting for approval... ({attempts}/{maxWaitAttempts})");
            }

            if (receipt == null)
            {
                throw new Exception("Approval timed out");
            }

            if (receipt.Status.Value == 0)  // or check receipt.Failed
            {
                Debug.LogError($"Approval transaction REVERTED on-chain! Tx hash: {approveTxHash}");
                throw new Exception("Approval reverted on-chain (status = 0)");
            }

            Debug.Log("✓ Approval confirmed");

            var allowanceFunction = tokenContract.GetFunction("allowance");
            var allowance = await allowanceFunction.CallAsync<BigInteger>(
                walletAddress,
                UNISWAP_ROUTER_ADDRESS
            );
            Debug.Log($"Current allowance: {allowance}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Approval failed: {ex}");
            throw;
        }
    }

    private async Task<BigInteger> GetAmountOut(Web3 web3, BigInteger amountIn, string tokenIn, string tokenOut)
    {
        try
        {
            Debug.Log($"Calling getAmountsOut...");

            var routerContract = web3.Eth.GetContract(ROUTER_ABI, UNISWAP_ROUTER_ADDRESS);
            var getAmountsOutFunction = routerContract.GetFunction("getAmountsOut");

            var path = new[] { tokenIn, tokenOut };
            var amounts = await getAmountsOutFunction.CallAsync<List<BigInteger>>(amountIn, path);

            if (amounts == null || amounts.Count == 0)
            {
                Debug.LogError("❌ getAmountsOut returned null or empty!");
                return BigInteger.Zero;
            }

            BigInteger amountOut = amounts[amounts.Count - 1];

            Debug.Log($"✓ getAmountsOut Success! Amount Out: {Web3.Convert.FromWei(amountOut, 18)}");

            return amountOut;
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ Error in GetAmountOut: {ex.Message}");
            return BigInteger.Zero;
        }
    }

    private async Task<string> ExecuteSwap(Web3 web3, string walletAddress, BigInteger amountIn, BigInteger amountOutMin, string tokenIn, string tokenOut)
    {
        try
        {
            Debug.Log($"=== EXECUTING SWAP ===");
            Debug.Log($"AmountIn: {amountIn}");
            Debug.Log($"AmountOutMin: {amountOutMin}");

            var routerContract = web3.Eth.GetContract(ROUTER_ABI, UNISWAP_ROUTER_ADDRESS);
            var swapFunction = routerContract.GetFunction("swapExactTokensForTokens");

            BigInteger deadline = BigInteger.Parse(DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()) + (20 * 60);
            var path = new[] { tokenIn, tokenOut };

            if (!IsValidEthereumAddress(tokenIn) || !IsValidEthereumAddress(tokenOut))
            {
                throw new Exception("Invalid token address format");
            }

            // Estimate gas
            var swapGas = await swapFunction.EstimateGasAsync(
                walletAddress,
                null,
                null,
                amountIn,
                amountOutMin,
                path,
                walletAddress,
                deadline
            );

            var gasWithBuffer = new HexBigInteger(swapGas.Value + (swapGas.Value / 5));
            Debug.Log($"Gas estimate: {swapGas.Value}, with buffer: {gasWithBuffer.Value}");

            var gasPrice = await web3.Eth.GasPrice.SendRequestAsync();
            var gasPriceWithBuffer = new HexBigInteger(gasPrice.Value + (gasPrice.Value / 10));
            Debug.Log($"Estimated gas price: {gasPriceWithBuffer}");

            // Execute swap
            var swapTxHash = await swapFunction.SendTransactionAsync(
                walletAddress,
                gasWithBuffer,
                gasPriceWithBuffer,
                null,
                amountIn,
                amountOutMin,
                path,
                walletAddress,
                deadline
            );

            Debug.Log($"✓ Swap transaction sent: {swapTxHash}");

            // Wait for confirmation
            var receipt = await web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(swapTxHash);
            int maxWaitAttempts = 30;
            int attempts = 0;

            while (receipt == null && attempts < maxWaitAttempts)
            {
                await Task.Delay(2000);
                receipt = await web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(swapTxHash);
                attempts++;
            }

            if (receipt != null)
            {
                Debug.Log($"Receipt status: {receipt.Status.Value}");

                if (receipt.Status.Value == 1)
                {
                    Debug.Log("✓ Swap confirmed successfully!");
                    return swapTxHash;
                }
                else
                {
                    Debug.LogError($"Swap failed in transaction. Gas used: {receipt.GasUsed.Value}");
                    return "";
                }
            }
            else
            {
                Debug.LogError("No receipt received");
                return "";
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Swap execution failed: {ex.Message}");
            throw;
        }
    }

    private bool IsValidEthereumAddress(string address)
    {
        if (string.IsNullOrEmpty(address)) return false;
        return address.StartsWith("0x") && address.Length == 42;
    }

    private async Task<(BigInteger reserveA, BigInteger reserveB)> GetReserves(Web3 web3, string pairAddress, string pairABI)
    {
        try
        {
            var pairContract = web3.Eth.GetContract(pairABI, pairAddress);
            var getReservesFunction = pairContract.GetFunction("getReserves");

            var callInput = getReservesFunction.CreateCallInput();
            var result = await web3.Eth.Transactions.Call.SendRequestAsync(callInput);

            if (result == null || result == "0x")
            {
                Debug.LogError("getReserves returned null or empty");
                return (BigInteger.Zero, BigInteger.Zero);
            }

            var hexString = result.Replace("0x", "");

            if (hexString.Length < 128)
            {
                Debug.LogError($"Invalid hex string length: {hexString.Length}");
                return (BigInteger.Zero, BigInteger.Zero);
            }

            string reserve0Hex = hexString.Substring(0, 64);
            string reserve1Hex = hexString.Substring(64, 64);

            BigInteger reserve0 = BigInteger.Parse(reserve0Hex, System.Globalization.NumberStyles.HexNumber);
            BigInteger reserve1 = BigInteger.Parse(reserve1Hex, System.Globalization.NumberStyles.HexNumber);

            return (reserve0, reserve1);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error getting reserves: {ex.Message}");
            return (BigInteger.Zero, BigInteger.Zero);
        }
    }

    public void Back()
    {
        SceneManager.LoadScene("Scene1");
    }

    private async Task CheckBalance()
    {
        try
        {
            var web3 = SDKManager.Instance.Web3;

            // Debug.Log($"mytokenadd: {ABIManager.mytokenAddress} othertokenAdd: { ABIManager.othertokenAddress } ");

            // Load token contracts
            var token0Contract = web3.Eth.GetContract(ABIManager.mytokenABI, ABIManager.mytokenAddress);
            var token1Contract = web3.Eth.GetContract(ABIManager.mytokenABI, ABIManager.othertokenAddress);

            BigInteger balance0 = await token0Contract.GetFunction("balanceOf")
                .CallAsync<BigInteger>(SDKManager.Instance.walletAddress);
            BigInteger balance1 = await token1Contract.GetFunction("balanceOf")
                .CallAsync<BigInteger>(SDKManager.Instance.walletAddress);

            var normalizedBalance0 = Web3.Convert.FromWei(balance0);
            var normalizedBalance1 = Web3.Convert.FromWei(balance1);

            // Debug.Log($"Balances: balance0={balance0}, balance1={balance1}");
            Debug.Log($"Balances: token0={normalizedBalance0}, token1={normalizedBalance1}");

        }
        catch (Exception ex)
        {
            Debug.LogError("Error calling the contract: " + ex.Message);
        }
    }

}
