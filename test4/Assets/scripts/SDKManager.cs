using UnityEngine;
using UnityEngine.SceneManagement;

using System;
using System.Threading.Tasks;
using System.Numerics;

using Nethereum.Web3;
using Nethereum.Hex.HexTypes;
using Nethereum.JsonRpc.Client;
using Nethereum.ABI.FunctionEncoding.Attributes;

using UnityEngine.Networking;

using Newtonsoft.Json.Linq;




public class SDKManager : MonoBehaviour
{
    public static SDKManager Instance{ get; private set; }
    public Web3 Web3 { get; private set; } 
    [SerializeField][HideInInspector] private string rpcUrl = "http://192.168.100.60:8545"; 

    [HideInInspector]public string contractAddress = "0xea2Fd44FBa043eBba814CB547334D935FaE7d88b";
    
    [TextArea][HideInInspector]
    public string abi = "[{\"inputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"constructor\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":false,\"internalType\":\"address\",\"name\":\"sender\",\"type\":\"address\"},{\"indexed\":false,\"internalType\":\"uint256\",\"name\":\"playerId\",\"type\":\"uint256\"},{\"indexed\":false,\"internalType\":\"string\",\"name\":\"wallet\",\"type\":\"string\"},{\"indexed\":false,\"internalType\":\"uint256\",\"name\":\"coin\",\"type\":\"uint256\"},{\"indexed\":false,\"internalType\":\"uint256\",\"name\":\"character\",\"type\":\"uint256\"}],\"name\":\"ScoreSubmitted\",\"type\":\"event\"},{\"inputs\":[],\"name\":\"ethPrice\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"stateMutability\":\"view\",\"type\":\"function\",\"constant\":true},{\"inputs\":[],\"name\":\"maxClamp\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"stateMutability\":\"view\",\"type\":\"function\",\"constant\":true},{\"inputs\":[],\"name\":\"midClamp\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"stateMutability\":\"view\",\"type\":\"function\",\"constant\":true},{\"inputs\":[],\"name\":\"minClamp\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"stateMutability\":\"view\",\"type\":\"function\",\"constant\":true},{\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"name\":\"playerAddresses\",\"outputs\":[{\"internalType\":\"address\",\"name\":\"\",\"type\":\"address\"}],\"stateMutability\":\"view\",\"type\":\"function\",\"constant\":true},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"\",\"type\":\"address\"}],\"name\":\"playerScores\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"playerId\",\"type\":\"uint256\"},{\"internalType\":\"string\",\"name\":\"walletAddress\",\"type\":\"string\"},{\"internalType\":\"uint256\",\"name\":\"coinAmount\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"characterIndex\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"exchangedAmount\",\"type\":\"uint256\"},{\"internalType\":\"bool\",\"name\":\"exists\",\"type\":\"bool\"}],\"stateMutability\":\"view\",\"type\":\"function\",\"constant\":true},{\"inputs\":[],\"name\":\"hello\",\"outputs\":[{\"internalType\":\"string\",\"name\":\"\",\"type\":\"string\"}],\"stateMutability\":\"pure\",\"type\":\"function\",\"constant\":true},{\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"_price\",\"type\":\"uint256\"}],\"name\":\"setPrice\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"playerId\",\"type\":\"uint256\"},{\"internalType\":\"string\",\"name\":\"add\",\"type\":\"string\"},{\"internalType\":\"uint256\",\"name\":\"coinAmount\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"characterIndex\",\"type\":\"uint256\"}],\"name\":\"Sendscore\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"_newCoinAmount\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"_newCharacterIndex\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"_exchangedAmount\",\"type\":\"uint256\"}],\"name\":\"updateScoreByPlayerId\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"_amount\",\"type\":\"uint256\"}],\"name\":\"setExchangeAmount\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"getAllData\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"},{\"components\":[{\"internalType\":\"uint256\",\"name\":\"playerId\",\"type\":\"uint256\"},{\"internalType\":\"string\",\"name\":\"walletAddress\",\"type\":\"string\"},{\"internalType\":\"uint256\",\"name\":\"coinAmount\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"characterIndex\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"exchangedAmount\",\"type\":\"uint256\"},{\"internalType\":\"bool\",\"name\":\"exists\",\"type\":\"bool\"}],\"internalType\":\"struct TestContract.Score[]\",\"name\":\"\",\"type\":\"tuple[]\"}],\"stateMutability\":\"view\",\"type\":\"function\",\"constant\":true},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"_wallet\",\"type\":\"address\"}],\"name\":\"getPlayerData\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"playerId\",\"type\":\"uint256\"},{\"internalType\":\"string\",\"name\":\"walletAddress\",\"type\":\"string\"},{\"internalType\":\"uint256\",\"name\":\"coinAmount\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"characterIndex\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"exchangedAmount\",\"type\":\"uint256\"},{\"internalType\":\"bool\",\"name\":\"exists\",\"type\":\"bool\"}],\"stateMutability\":\"view\",\"type\":\"function\",\"constant\":true},{\"inputs\":[],\"name\":\"getClamps\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"stateMutability\":\"view\",\"type\":\"function\",\"constant\":true},{\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"_min\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"_mid\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"_max\",\"type\":\"uint256\"}],\"name\":\"setModifiers\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"}]";


    [HideInInspector] public string walletAddress = "0x4aB5E0D87B8f27036a472ACAe9D7FbD34af4F51c";
    [HideInInspector] public string walletAddress2 = "0xEeC5bD1f87A918E56644DD60c09848B88Ec410e7";
    [HideInInspector] public string walletAddress3 = "0x5cdC304b37cF49A5FF9cbA6C59c8A8529F443164";
    [HideInInspector] public string walletAddress4 = "0x7BE00eD244D11b14ff184a0dA503A9A4659D3A41";
    [HideInInspector] public string walletAddress5 = "0xD4c9b0818E0FEcCa7aC2096749eDA754a9c9c857";

    public int selectedCharacterIndex = 0; // default to first character
    public BigInteger totalCoin = 0;
    public int playerId = 1;

    public double minModifier = 0.0f;
    public double maxModifier = 0.0f;
    public double midModifier = 0.0f;
    
    [HideInInspector]public double priceUsd = 0;
    [HideInInspector]public decimal marketPrice = 0;
    [HideInInspector]public double TotalExchangeableToken = 0;
    [HideInInspector]public double TotalToken = 0;

    [HideInInspector]public double modifier1 = 0;
    [HideInInspector]public double modifierOne = 0;
    [HideInInspector]public double modifierTwo = 0;
    [HideInInspector]public double modifierThree = 0;

    [HideInInspector] public string testRpcUrl = "https://mainnet.infura.io/v3/3a57c44201504fff96c533b1b2c3515f";
    [HideInInspector] public string testAbi = @"[
        {
            ""constant"": true,
            ""inputs"": [],
            ""name"": ""getReserves"",
            ""outputs"": [
                { ""internalType"": ""uint112"", ""name"": ""reserve0"", ""type"": ""uint112"" },
                { ""internalType"": ""uint112"", ""name"": ""reserve1"", ""type"": ""uint112"" },
                { ""internalType"": ""uint32"", ""name"": ""blockTimestampLast"", ""type"": ""uint32"" }
            ],
            ""payable"": false,
            ""stateMutability"": ""view"",
            ""type"": ""function""
        }
    ]";

   // slp/weth
    [HideInInspector] public string pairContractAddress = "0x3dDBbFd2CF0120F6E98Ea0D44AeD3475C385F0E9";
    [HideInInspector] public string ohlcvUrl = "https://api.geckoterminal.com/api/v2/networks/ronin/pools/0x306a28279d04a47468ed83d55088d0dcd1369294/ohlcv";
    [HideInInspector] public string dexscreenerApi = "https://api.coingecko.com/api/v3/simple/price?ids=smooth-love-potion&vs_currencies=usd";


    // GST/WSOL not right informations
    // [HideInInspector] public string pairContractAddress = "0x3dDBbFd2CF0120F6E98Ea0D44AeD3475C385F0E9";
    // [HideInInspector] public string ohlcvUrl = "https://api.geckoterminal.com/api/v2/networks/solana/pools/2ko9dfZVkCehcw7iY8zyQ5qA5YDLzyPh2etZgJrsCufk/ohlcv";
    // [HideInInspector] public string dexscreenerApi = "https://api.coingecko.com/api/v3/simple/price?ids=smooth-love-potion&vs_currencies=usd";


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        Web3 = new Web3(rpcUrl); // Create Web3 once here
        Debug.Log("Web3 connected to: " + rpcUrl + " web3 " + Web3 );
    }

    public async Task FetchModifierClamps()
    {
        string abitest = "[{\"inputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"constructor\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":false,\"internalType\":\"address\",\"name\":\"sender\",\"type\":\"address\"},{\"indexed\":false,\"internalType\":\"uint256\",\"name\":\"playerId\",\"type\":\"uint256\"},{\"indexed\":false,\"internalType\":\"string\",\"name\":\"wallet\",\"type\":\"string\"},{\"indexed\":false,\"internalType\":\"uint256\",\"name\":\"coin\",\"type\":\"uint256\"},{\"indexed\":false,\"internalType\":\"uint256\",\"name\":\"character\",\"type\":\"uint256\"}],\"name\":\"ScoreSubmitted\",\"type\":\"event\"},{\"inputs\":[],\"name\":\"ethPrice\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"stateMutability\":\"view\",\"type\":\"function\",\"constant\":true},{\"inputs\":[],\"name\":\"maxClamp\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"stateMutability\":\"view\",\"type\":\"function\",\"constant\":true},{\"inputs\":[],\"name\":\"midClamp\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"stateMutability\":\"view\",\"type\":\"function\",\"constant\":true},{\"inputs\":[],\"name\":\"minClamp\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"stateMutability\":\"view\",\"type\":\"function\",\"constant\":true},{\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"name\":\"playerAddresses\",\"outputs\":[{\"internalType\":\"address\",\"name\":\"\",\"type\":\"address\"}],\"stateMutability\":\"view\",\"type\":\"function\",\"constant\":true},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"\",\"type\":\"address\"}],\"name\":\"playerScores\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"playerId\",\"type\":\"uint256\"},{\"internalType\":\"string\",\"name\":\"walletAddress\",\"type\":\"string\"},{\"internalType\":\"uint256\",\"name\":\"coinAmount\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"characterIndex\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"exchangedAmount\",\"type\":\"uint256\"},{\"internalType\":\"bool\",\"name\":\"exists\",\"type\":\"bool\"}],\"stateMutability\":\"view\",\"type\":\"function\",\"constant\":true},{\"inputs\":[],\"name\":\"hello\",\"outputs\":[{\"internalType\":\"string\",\"name\":\"\",\"type\":\"string\"}],\"stateMutability\":\"pure\",\"type\":\"function\",\"constant\":true},{\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"_price\",\"type\":\"uint256\"}],\"name\":\"setPrice\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"playerId\",\"type\":\"uint256\"},{\"internalType\":\"string\",\"name\":\"add\",\"type\":\"string\"},{\"internalType\":\"uint256\",\"name\":\"coinAmount\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"characterIndex\",\"type\":\"uint256\"}],\"name\":\"Sendscore\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"_newCoinAmount\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"_newCharacterIndex\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"_exchangedAmount\",\"type\":\"uint256\"}],\"name\":\"updateScoreByPlayerId\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"_amount\",\"type\":\"uint256\"}],\"name\":\"setExchangeAmount\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"getAllData\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"},{\"components\":[{\"internalType\":\"uint256\",\"name\":\"playerId\",\"type\":\"uint256\"},{\"internalType\":\"string\",\"name\":\"walletAddress\",\"type\":\"string\"},{\"internalType\":\"uint256\",\"name\":\"coinAmount\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"characterIndex\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"exchangedAmount\",\"type\":\"uint256\"},{\"internalType\":\"bool\",\"name\":\"exists\",\"type\":\"bool\"}],\"internalType\":\"struct TestContract.Score[]\",\"name\":\"\",\"type\":\"tuple[]\"}],\"stateMutability\":\"view\",\"type\":\"function\",\"constant\":true},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"_wallet\",\"type\":\"address\"}],\"name\":\"getPlayerData\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"playerId\",\"type\":\"uint256\"},{\"internalType\":\"string\",\"name\":\"walletAddress\",\"type\":\"string\"},{\"internalType\":\"uint256\",\"name\":\"coinAmount\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"characterIndex\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"exchangedAmount\",\"type\":\"uint256\"},{\"internalType\":\"bool\",\"name\":\"exists\",\"type\":\"bool\"}],\"stateMutability\":\"view\",\"type\":\"function\",\"constant\":true},{\"inputs\":[],\"name\":\"getClamps\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"stateMutability\":\"view\",\"type\":\"function\",\"constant\":true},{\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"_min\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"_mid\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"_max\",\"type\":\"uint256\"}],\"name\":\"setModifiers\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"}]";
        string contractAddresstest = "0xea2Fd44FBa043eBba814CB547334D935FaE7d88b";
        
        var contract = Web3.Eth.GetContract(abitest, contractAddresstest);
        Debug.Log("abi " + abitest + "contractAddress " + contractAddresstest );

        var ModFunction = contract.GetFunction("getClamps");

        try
        {
            var result = await ModFunction.CallDeserializingToObjectAsync<ClampOutputDTO>();  

            minModifier = result.minClamp / 100.0;
            midModifier = result.midClamp / 100.0;
            maxModifier = result.maxClamp / 100.0;

            Debug.Log($"Fetched min: {minModifier} mid: {midModifier} max: {maxModifier}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Error fetching modifier clamps: " + ex.Message);
        }
    }

    private async void Start()
    {
        await FetchModifierClamps();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    

}

[FunctionOutput]
public class ClampOutputDTO : IFunctionOutputDTO
{
    [Parameter("uint256", "minClamp", 1)]
    public uint minClamp { get; set; }

    [Parameter("uint256", "midClamp", 2)]
    public uint midClamp { get; set; }

    [Parameter("uint256", "maxClamp", 3)]
    public uint maxClamp { get; set; }
}
