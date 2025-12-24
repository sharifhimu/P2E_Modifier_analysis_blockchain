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

    public static string tokenpairAddress = "0x8ca963C4E56C5E873530D0993ee22030C32F0cc8";
	public static string mytokenAddress = "0x338E5a41da5EfF93236f295228d4bFAda8fd4d50";
	public static string othertokenAddress = "0x19b64588e861B0Ce0440C85B4E35BD02909bd7fc";
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