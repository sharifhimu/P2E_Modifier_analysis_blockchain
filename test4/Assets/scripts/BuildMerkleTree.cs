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
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.ABI.Encoders;  
using Nethereum.ABI.FunctionEncoding;
using Nethereum.ABI;
using Nethereum.Util;




public class BuildMerkleTree : MonoBehaviour
{

    public static BuildMerkleTree Instance { get; private set; }

    // Read balances for all addresses in the list
    public async Task CheckAndSaveBalancesAsync(List<string> playerAddresses)
    {
        try
        {
            var web3 = SDKManager.Instance.Web3Owner;


            var token0Contract = web3.Eth.GetContract(ABIManager.mytokenABI, ABIManager.mytokenAddress);
            var token1Contract = web3.Eth.GetContract(ABIManager.othertokenABI, ABIManager.othertokenAddress);

            var balanceOfToken0 = token0Contract.GetFunction("balanceOf");
            var balanceOfToken1 = token1Contract.GetFunction("balanceOf");

            // Safe allowance from pair contract
            var pairContract = web3.Eth.GetContract(ABIManager.tokenpairABI, ABIManager.tokenpairAddress);
            var fn = pairContract.GetFunction("computeAllowance");

            List<(string address, string bal0, string bal1, string allowance)> playerInfos = new();

            foreach (var address in playerAddresses)
            {
                var safeAllowanceHex = await fn.CallAsync<BigInteger>(address);
                var safeAllowance = Web3.Convert.FromWei(safeAllowanceHex).ToString();
                Debug.Log($" address: {address} safeAllowance: {safeAllowance} ");

                var bal0Hex = await balanceOfToken0.CallAsync<BigInteger>(address);
                var bal1Hex = await balanceOfToken1.CallAsync<BigInteger>(address);

                // Debug.Log($" bal0: {bal0Hex}, bal1: {bal1Hex}, address: {address} ");


                var bal0 = Web3.Convert.FromWei(bal0Hex).ToString();
                var bal1 = Web3.Convert.FromWei(bal1Hex).ToString();

                // Save allowance into SDKManager.Instance
                SDKManager.Instance.playerAllowences[address.ToLowerInvariant()] = safeAllowanceHex;

                playerInfos.Add((address, bal0, bal1, safeAllowanceHex.ToString()));

            }


            // 🔹 Fetch epochDuration & compute current epochId
            var epochIdFn = pairContract.GetFunction("currentEpochId");
            var epochId = await epochIdFn.CallAsync<BigInteger>();
            Debug.Log($" Current EpochId from sdkmanager taken from currentEpochId contract function: {epochId}");
            SDKManager.Instance.CurrentEpochId = epochId;

            // 🔹 Build Merkle Tree (epochId + address + allowance)
            var root = MakeMerkleTree(playerInfos, epochId);

            Debug.Log("SDKManager.Instance.ownerAddress " + SDKManager.Instance.ownerAddress);
            // 🔹 Call setMerkleRoot(epochId, root)
            var rootBytes32 = ("0x" + root).HexToByteArray();
            var setMerkleRootFn = pairContract.GetFunction("setMerkleRoot");
            var txReceipt = await setMerkleRootFn.SendTransactionAndWaitForReceiptAsync(
                from: SDKManager.Instance.ownerAddress,
                gas: new Nethereum.Hex.HexTypes.HexBigInteger(600000),
                value: null,
                functionInput: new object[] { epochId, rootBytes32 }
            );

            DataLogger.Instance.LogOperation("SetMerkleRoot", txReceipt.TransactionHash, SDKManager.Instance.ownerAddress);

            // Debug.Log("setMerkleRoot TxHash: " + txReceipt.TransactionHash);
            if (!String.IsNullOrEmpty(txReceipt.TransactionHash))
            {
                Scene currentScene = SceneManager.GetActiveScene();
                // Scene currentScene = EditorSceneManager.OpenScene();
                string sceneName = currentScene.name;
                if (sceneName == "Scene0")
                {
                    SceneManager.LoadScene("Scene1");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Error in CheckAndSaveBalancesAsync: " + ex.Message);
        }
    }

    public string MakeMerkleTree( List<(string address, string bal0, string bal1, string allowance)> playerInfos, BigInteger epochId  ){

        var leaves = playerInfos.Select(p =>
        {
            //Debug.Log($"sdkmanager: bal1: {p.bal0}, allowence: { Web3.Convert.FromWei(BigInteger.Parse(p.allowance)) }, address: { p.address } ");
            BigInteger allowanceWei = BigInteger.Parse(p.allowance);
        
            var encoder = new ABIEncode();
            var encoded = encoder.GetABIEncodedPacked(
                new ABIValue("uint256", epochId),
                new ABIValue("address", p.address.ToLowerInvariant()),
                new ABIValue("uint256", allowanceWei)
            );

            var leaf = Sha3Keccack.Current.CalculateHash(encoded);
            return leaf;
        }).ToList();

        var tree = new MerkleTree(leaves);

        //Debug.Log($" tree: {tree} ");
        //Debug.Log($" leaves: {leaves} ");
        // Debug.Log($" epochId: {epochId} ");

        SDKManager.Instance.CurrentMerkleTree = tree;
        SDKManager.Instance.CurrentLeaves = leaves;

        string root = BitConverter.ToString(tree.GetRoot()).Replace("-", "");
        Debug.Log("Merkle Root: " + root);
        return root;

    }
}
