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
    public static string tokenpairAddress = "0x625a6FDf990a8c6425bc6CB0638a7fc677eAf6f7";  // "0x7A92cE7C7A8A68D7EE6873f9c89878761cAD7413";
    public static string mytokenAddress = "0xc259a7FF7F48F0bF95DE77c8dC4c31be9F41362f";  // "0x66c16D0e4bf6048165D041e29A43d2c93E04168e";
    public static string othertokenAddress = "0xB1A233A9f7f37CaB49391664F7c4a4892F4BA1AB"; // "0x90d2ae79828a13B9d74F6A48E1C01D603938237c";

    // otk 0x19b64588e861B0Ce0440C85B4E35BD02909bd7fc
    // myt 0x338E5a41da5EfF93236f295228d4bFAda8fd4d50

    //ganache
    //public static string tokenpairAddress = "0x8Df8801b6a2491C6FFdc6A939AeE22A55eA84398";
    //public static string mytokenAddress = "0x0DcF19699901AEd35bE3c81d3B7b13E7abF11b76";
    //public static string othertokenAddress = "0xEbB493d0007E78e150A496d1ABcf8614a7D93138";


    public static string uniswapRouterAddress = "0xeE567Fe1712Faf6149d80dA1E6934E354124CfE3"; // Sepolia V2 Router
    // 0xeE567Fe1712Faf6149d80dA1E6934E354124CfE3 0xC532a74256D3Db42D0Bf7a0489b1d663F30D4E7d
    public static string uniswapPairAddress = "0xaB9D9f5d052769BE43309B106BEBB3cB0923d19e"; // "0x4c88Bf9dAd5002303104059fAfD96Ed26727061a";  // "0xaB9D9f5d052769BE43309B106BEBB3cB0923d19e";
    public static string uniswapPairMYT = "0xc259a7FF7F48F0bF95DE77c8dC4c31be9F41362f"; // "0x338E5a41da5EfF93236f295228d4bFAda8fd4d50"; // "0xc259a7FF7F48F0bF95DE77c8dC4c31be9F41362f";
    public static string uniswapPairOTK = "0xB1A233A9f7f37CaB49391664F7c4a4892F4BA1AB"; // "0x19b64588e861B0Ce0440C85B4E35BD02909bd7fc"; // "0xB1A233A9f7f37CaB49391664F7c4a4892F4BA1AB";


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