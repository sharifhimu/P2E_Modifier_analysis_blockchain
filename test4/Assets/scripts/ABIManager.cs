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

    public static string tokenpairAddress = "0x4710A48FD1A25F9799B39d27a1C530221344d164";
	public static string mytokenAddress = "0x424a5038b60f3D91E0a37a33AEA47EE10391BFBD";
	public static string othertokenAddress = "0x6B8Be50757C448EFD031799Af522732517e7Aa42";


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