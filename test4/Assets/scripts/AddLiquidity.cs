using UnityEngine;
using TMPro;
using System.Numerics;
using System;
using System.Threading.Tasks;

using UnityEngine.SceneManagement;

using Nethereum.Hex.HexTypes;
using Nethereum.Web3;
using Nethereum.Contracts;
using Nethereum.RPC.Eth.DTOs;


public class AddLiquidity : MonoBehaviour
{
    public TMP_InputField token0InputField;
    public TMP_InputField token1InputField;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Back(){
        SceneManager.LoadScene("Scene2");
    }

    public async void Add()
    {
        try
        {
            var web3 = SDKManager.Instance.Web3;

            // Parse user input
            BigInteger token0Input = BigInteger.Parse(token0InputField.text);
            BigInteger token1Input = BigInteger.Parse(token1InputField.text);

            // Load token contracts
            var token0Contract = web3.Eth.GetContract(ABIManager.mytokenABI, ABIManager.mytokenAddress);
            var token1Contract = web3.Eth.GetContract(ABIManager.mytokenABI, ABIManager.othertokenAddress);

            // Get decimals
            int decimals0 = await token0Contract.GetFunction("decimals").CallAsync<int>();
            int decimals1 = await token1Contract.GetFunction("decimals").CallAsync<int>();

            // Scale to token units
            BigInteger token0Amount = token0Input * BigInteger.Pow(10, decimals0);
            BigInteger token1Amount = token1Input * BigInteger.Pow(10, decimals1);

            // Get reserves
            var pairContract = web3.Eth.GetContract(ABIManager.tokenpairABI, ABIManager.tokenpairAddress);
            var reserves = await pairContract.GetFunction("getReserves").CallDeserializingToObjectAsync<Reserves>();

            Debug.Log($"Reserve0: {reserves.Reserve0}, Reserve1: {reserves.Reserve1}");

            // If pool already has liquidity, adjust ratio
            if (reserves.Reserve0 > 0 && reserves.Reserve1 > 0)
            {
                token1Amount = (token0Amount * reserves.Reserve1) / reserves.Reserve0;
                Debug.Log($"Adjusted token1Amount to pool ratio: {token1Amount}");
            }
            else
            {
                Debug.Log("No liquidity in pool — using provided amounts as initial supply.");
            }

            // Check wallet balances
            BigInteger balance0 = await token0Contract.GetFunction("balanceOf")
                .CallAsync<BigInteger>(SDKManager.Instance.walletAddress);
            BigInteger balance1 = await token1Contract.GetFunction("balanceOf")
                .CallAsync<BigInteger>(SDKManager.Instance.walletAddress);

            Debug.Log($"Balances: token0={balance0}, token1={balance1}");

            if (balance0 < token0Amount)
            {
                Debug.LogError($"Insufficient token0 balance. balance: {balance0} token0: {token0Amount} " );
                return;
            }
            if (balance1 < token1Amount)
            {
                Debug.LogError($"Insufficient token1 balance. balance: {balance1} token1: {token1Amount} ");
                return;
            }

            // Approve if needed
            await ApproveIfNeeded(web3, token0Contract, SDKManager.Instance.walletAddress, ABIManager.tokenpairAddress, token0Amount);
            await ApproveIfNeeded(web3, token1Contract, SDKManager.Instance.walletAddress, ABIManager.tokenpairAddress, token1Amount);

            // Add liquidity
            var addLiquidityFunc = pairContract.GetFunction("addLiquidity");

            var gasAdd = await addLiquidityFunc.EstimateGasAsync(
                SDKManager.Instance.walletAddress, null, null, token0Amount, token1Amount);
            gasAdd = new HexBigInteger(gasAdd.Value + (gasAdd.Value / 5)); // +20% buffer

            var tx = await addLiquidityFunc.SendTransactionAsync(
                SDKManager.Instance.walletAddress, gasAdd, null, token0Amount, token1Amount);

            Debug.Log("Liquidity added! TxHash: " + tx);
        }
        catch (Exception ex)
        {
            Debug.LogError("Error adding liquidity: " + ex.Message);
        }
    }

    // Helper: approve only if needed
    private async Task ApproveIfNeeded(Web3 web3, Contract tokenContract, string owner, string spender, BigInteger amount)
    {
        var allowanceFunc = tokenContract.GetFunction("allowance");
        BigInteger currentAllowance = await allowanceFunc.CallAsync<BigInteger>(owner, spender);

        if (currentAllowance >= amount)
        {
            Debug.Log($"Already approved: {tokenContract.Address}");
            return;
        }

        var approveFunc = tokenContract.GetFunction("approve");
        var gas = await approveFunc.EstimateGasAsync(owner, null, null, spender, amount);
        gas = new HexBigInteger(gas.Value + (gas.Value / 5));

        var tx = await approveFunc.SendTransactionAsync(owner, gas, null, spender, amount);
        Debug.Log($"Approved {tokenContract.Address} for {amount}, Tx: {tx}");
    }




}
