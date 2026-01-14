using UnityEngine;
using System;
using Newtonsoft.Json.Linq;
using Nethereum.Web3;
using System.Threading.Tasks;


public class ABIManager : MonoBehaviour
{

	public static string tokenpairABI;
	public static string mytokenABI;
	public static string othertokenABI;
	public static string uniswapV2ABI;
	public static string uniswapV2TokenPairABI;

    // testnet
    public static string tokenpairAddress = "0xe6BA29878D23F1ea3d9122607Cfdad396B134ED0";
    public static string mytokenAddress = "0x85c4425A0aa3b351EC51049ab3E03b4738423562";
    public static string othertokenAddress = "0xb4a8f37F2a43b96284707226E509675630d8038f";

    //ganache
    //public static string tokenpairAddress = "0x8Df8801b6a2491C6FFdc6A939AeE22A55eA84398";
    //public static string mytokenAddress = "0x0DcF19699901AEd35bE3c81d3B7b13E7abF11b76";
    //public static string othertokenAddress = "0xEbB493d0007E78e150A496d1ABcf8614a7D93138";


    public static string uniswapRouterAddress = "0xeE567Fe1712Faf6149d80dA1E6934E354124CfE3"; // Sepolia V2 Router
    // 0xeE567Fe1712Faf6149d80dA1E6934E354124CfE3 0xC532a74256D3Db42D0Bf7a0489b1d663F30D4E7d
    public static string uniswapPairAddress = "0x4c88Bf9dAd5002303104059fAfD96Ed26727061a";

    void Awake()
    {
        LoadABI1();
        LoadABI2();
        LoadABI3();
        LoadABI4();
        LoadABI5();
        // Only proceed if both ABIs are loaded
        if (string.IsNullOrEmpty(tokenpairABI) || 
            string.IsNullOrEmpty(mytokenABI) || 
            string.IsNullOrEmpty(othertokenABI) || 
            string.IsNullOrEmpty(uniswapV2ABI) ||
            string.IsNullOrEmpty(uniswapV2TokenPairABI)
        )
        {
            Debug.LogError("ABIs not loaded, cannot fetch token addresses");
            return;
        }

    }

    [Serializable]
    public class AbiFileWrapper<T>
    {
        public T[] abi;
    }

    void LoadABI1()
    {
        TextAsset abiTextAsset = Resources.Load<TextAsset>("TokenPair");
        if (abiTextAsset != null)
        {

            // Parse the file and extract only the "abi" array
            var jObj = JObject.Parse(abiTextAsset.text);
            tokenpairABI = jObj["abi"].ToString(); // <-- this will be a valid JSON array

            //Debug.Log("tokenpair ABI loaded successfully: " + tokenpairABI);
        }
        else
        {
            Debug.LogError("Failed to load tokenpair ABI file");
        }
    }

    void LoadABI2()
    {
        TextAsset abiTextAsset = Resources.Load<TextAsset>("MyToken");
        if (abiTextAsset != null)
        {
            var jObj = JObject.Parse(abiTextAsset.text);
            mytokenABI = jObj["abi"].ToString();

            //Debug.Log("mytoken ABI loaded successfully: " + mytokenABI);
        }
        else
        {
            Debug.LogError("Failed to load mytoken ABI file");
        }
    }

    void LoadABI3()
    {
        TextAsset abiTextAsset = Resources.Load<TextAsset>("OtherToken");
        if (abiTextAsset != null)
        {
            var jObj = JObject.Parse(abiTextAsset.text);
            othertokenABI = jObj["abi"].ToString();

            //Debug.Log("mytoken ABI loaded successfully: " + mytokenABI);
        }
        else
        {
            Debug.LogError("Failed to load othertoken ABI file");
        }
    }

    void LoadABI4()
    {
        TextAsset abiTextAsset = Resources.Load<TextAsset>("UniswapV2");
        if (abiTextAsset != null)
        {
            var jObj = JObject.Parse(abiTextAsset.text);
            uniswapV2ABI = jObj["abi"].ToString();

            //Debug.Log("mytoken ABI loaded successfully: " + mytokenABI);
        }
        else
        {
            Debug.LogError("Failed to load uniswap ABI file");
        }
    }

    void LoadABI5()
    {
        TextAsset abiTextAsset = Resources.Load<TextAsset>("UniswapV2TokenPair");
        if (abiTextAsset != null)
        {
            var jObj = JObject.Parse(abiTextAsset.text);
            uniswapV2TokenPairABI = jObj["abi"].ToString();

            //Debug.Log("mytoken ABI loaded successfully: " + mytokenABI);
        }
        else
        {
            Debug.LogError("Failed to load uniswap tokenpair ABI file");
        }
    }



}