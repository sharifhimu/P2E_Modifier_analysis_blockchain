using UnityEngine;
using UnityEngine.SceneManagement;

using System;
using System.Threading.Tasks;
using System.Numerics;

using Nethereum.Web3;
using Nethereum.Hex.HexTypes;
using Nethereum.JsonRpc.Client;

using UnityEngine.Networking;

using Newtonsoft.Json.Linq;


public class Scene1Manager : MonoBehaviour
{
    
    public async void GotoNextScene( int playerId ){
    
        switch(playerId){
            case 1:
                SDKManager.Instance.walletAddress = SDKManager.Instance.walletAddress;
                SDKManager.Instance.playerId = 1;
                break;
            case 2:
                SDKManager.Instance.walletAddress = SDKManager.Instance.walletAddress2;
                SDKManager.Instance.playerId = 2;
                break;
            case 3: 
                SDKManager.Instance.walletAddress = SDKManager.Instance.walletAddress3;
                SDKManager.Instance.playerId = 3;
                break;
            case 4: 
                SDKManager.Instance.walletAddress = SDKManager.Instance.walletAddress4;
                SDKManager.Instance.playerId = 4;
                break;
            case 5:
                SDKManager.Instance.walletAddress = SDKManager.Instance.walletAddress5;
                SDKManager.Instance.playerId = 5;
                break;
        }
        
        GetTokenPrice();
        SceneManager.LoadScene("Scene2");
    }

    private async Task GetTokenPrice()
    {
        string url = SDKManager.Instance.dexscreenerApi;
        UnityWebRequest www = UnityWebRequest.Get(url);
        await www.SendWebRequest();

        string priceNativeStr = "";
        double price = 0.0;

        if (www.result == UnityWebRequest.Result.Success)
        {
            string json = www.downloadHandler.text;
            var jObject = JObject.Parse(json);
            if( jObject.ContainsKey("pairs") ){
                 priceNativeStr = jObject["pairs"]?[0]?["priceNative"]?.ToString();
                 price = (double)jObject["pairs"]?[0]?["priceUsd"];
            }
            else {
                 price = (double)jObject["smooth-love-potion"]["usd"]; 
            }

            Debug.Log("price " + price );

            SDKManager.Instance.priceUsd = price;
            
        }
        else
        {
            Debug.LogError("Failed to fetch price: " + www.error);
        }
    }

}
