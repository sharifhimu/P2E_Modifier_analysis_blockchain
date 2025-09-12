using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Numerics;
using Nethereum.Web3;


public class Scene2Manager : MonoBehaviour
{
    public Button addLiquidityBtn;
    public Button swapTokenBtn;
    public Button backBtn;
    public Button checkReserveBtn;
    public Button checkBalanceBtn;

    private void OnEnable()
    {
        // Clear old listeners to prevent stacking
        addLiquidityBtn.onClick.RemoveAllListeners();
        swapTokenBtn.onClick.RemoveAllListeners();
        backBtn.onClick.RemoveAllListeners();
        checkReserveBtn.onClick.RemoveAllListeners();
        checkBalanceBtn.onClick.RemoveAllListeners();

        // Bind buttons
        addLiquidityBtn.onClick.AddListener(AddLiquidity);
        swapTokenBtn.onClick.AddListener(SwapToken);
        backBtn.onClick.AddListener(Back);
        checkReserveBtn.onClick.AddListener(CheckReserve);
        checkBalanceBtn.onClick.AddListener(CheckBalance);
    }

    public void AddLiquidity() => SceneManager.LoadScene("AddLiquidity");
    public void SwapToken() => SceneManager.LoadScene("SwapToken");
    public void Back() => SceneManager.LoadScene("Scene1");

    public async void CheckReserve()
    {
        try
        {
            var web3 = SDKManager.Instance.Web3;

            var pairContract = web3.Eth.GetContract(ABIManager.tokenpairABI, ABIManager.tokenpairAddress);
            var reserves = await pairContract.GetFunction("getReserves").CallDeserializingToObjectAsync<Reserves>();

            // Debug.Log($"Reserve0: {reserves.Reserve0}, Reserve1: {reserves.Reserve1}");

            var token0Contract = web3.Eth.GetContract(ABIManager.mytokenABI, ABIManager.mytokenAddress);
            var token1Contract = web3.Eth.GetContract(ABIManager.mytokenABI, ABIManager.othertokenAddress);

            int decimals0 = await token0Contract.GetFunction("decimals").CallAsync<int>();
            int decimals1 = await token1Contract.GetFunction("decimals").CallAsync<int>();

            BigInteger normalizedReserve0 = reserves.Reserve0 / BigInteger.Pow(10, decimals0);
            BigInteger normalizedReserve1 = reserves.Reserve1 / BigInteger.Pow(10, decimals1);

            Debug.Log($" normalizedReserve0: {normalizedReserve0}, normalizedReserve1: {normalizedReserve1} ");
        }
        catch (Exception ex)
        {
            Debug.LogError("Error calling the contract: " + ex.Message);
        }
    }

    public async void CheckBalance(){
        try{
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

        } catch (Exception ex)
        {
            Debug.LogError("Error calling the contract: " + ex.Message);
        }
    }
}
