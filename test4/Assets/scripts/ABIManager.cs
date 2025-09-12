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

    public static string tokenpairAddress = "0xA1936C3d0d4A7D0c3329CfAD2438f2AB1525E164";
	public static string mytokenAddress = "0x948d8d1083ac640090f75B3703e26dBf91f7ad7D";
	public static string othertokenAddress = "0x5A21aA0e64FF527FE58ff7Aeb2C67cE2460CCbbe";


    void Awake()
    {
        LoadABI1();
        LoadABI2();
        LoadABI3();
        // Only proceed if both ABIs are loaded
        if (string.IsNullOrEmpty(tokenpairABI) || string.IsNullOrEmpty(mytokenABI) || string.IsNullOrEmpty(othertokenABI)  )
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
            Debug.LogError("Failed to load mytoken ABI file");
        }
    }



}