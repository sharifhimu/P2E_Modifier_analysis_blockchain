using UnityEngine;
using TMPro;
using Nethereum.Hex.HexTypes;
using UnityEngine.SceneManagement;
using Nethereum.JsonRpc.Client;


public class InputManager : MonoBehaviour
{
    public TMP_InputField tokenInputField;

    string testcontractAddress = "0xA4e8331294C96EBcC29C6A2d577aB39E22BdAe8e";
    string testabi = @"[{""inputs"":[],""stateMutability"":""nonpayable"",""type"":""constructor""},{""anonymous"":false,""inputs"":[{""indexed"":false,""internalType"":""address"",""name"":""sender"",""type"":""address""},{""indexed"":false,""internalType"":""uint256"",""name"":""playerId"",""type"":""uint256""},{""indexed"":false,""internalType"":""string"",""name"":""wallet"",""type"":""string""},{""indexed"":false,""internalType"":""uint256"",""name"":""coin"",""type"":""uint256""},{""indexed"":false,""internalType"":""uint256"",""name"":""character"",""type"":""uint256""}],""name"":""ScoreSubmitted"",""type"":""event""},{""inputs"":[],""name"":""ethPrice"",""outputs"":[{""internalType"":""uint256"",""name"":"""",""type"":""uint256""}],""stateMutability"":""view"",""type"":""function"",""constant"":true},{""inputs"":[],""name"":""lastLiquidityUpdate"",""outputs"":[{""internalType"":""uint256"",""name"":"""",""type"":""uint256""}],""stateMutability"":""view"",""type"":""function"",""constant"":true},{""inputs"":[{""internalType"":""uint256"",""name"":"""",""type"":""uint256""}],""name"":""liquidityLog"",""outputs"":[{""internalType"":""uint256"",""name"":"""",""type"":""uint256""}],""stateMutability"":""view"",""type"":""function"",""constant"":true},{""inputs"":[],""name"":""maxClamp"",""outputs"":[{""internalType"":""uint256"",""name"":"""",""type"":""uint256""}],""stateMutability"":""view"",""type"":""function"",""constant"":true},{""inputs"":[],""name"":""midClamp"",""outputs"":[{""internalType"":""uint256"",""name"":"""",""type"":""uint256""}],""stateMutability"":""view"",""type"":""function"",""constant"":true},{""inputs"":[],""name"":""minClamp"",""outputs"":[{""internalType"":""uint256"",""name"":"""",""type"":""uint256""}],""stateMutability"":""view"",""type"":""function"",""constant"":true},{""inputs"":[{""internalType"":""uint256"",""name"":"""",""type"":""uint256""}],""name"":""playerAddresses"",""outputs"":[{""internalType"":""address"",""name"":"""",""type"":""address""}],""stateMutability"":""view"",""type"":""function"",""constant"":true},{""inputs"":[{""internalType"":""address"",""name"":"""",""type"":""address""}],""name"":""playerScores"",""outputs"":[{""internalType"":""uint256"",""name"":""playerId"",""type"":""uint256""},{""internalType"":""string"",""name"":""walletAddress"",""type"":""string""},{""internalType"":""uint256"",""name"":""coinAmount"",""type"":""uint256""},{""internalType"":""uint256"",""name"":""characterIndex"",""type"":""uint256""},{""internalType"":""uint256"",""name"":""exchangedAmount"",""type"":""uint256""},{""internalType"":""bool"",""name"":""exists"",""type"":""bool""}],""stateMutability"":""view"",""type"":""function"",""constant"":true},{""inputs"":[],""name"":""hello"",""outputs"":[{""internalType"":""string"",""name"":"""",""type"":""string""}],""stateMutability"":""pure"",""type"":""function"",""constant"":true},{""inputs"":[{""internalType"":""uint256"",""name"":""_price"",""type"":""uint256""}],""name"":""setPrice"",""outputs"":[],""stateMutability"":""nonpayable"",""type"":""function""},{""inputs"":[{""internalType"":""uint256"",""name"":""playerId"",""type"":""uint256""},{""internalType"":""string"",""name"":""add"",""type"":""string""},{""internalType"":""uint256"",""name"":""coinAmount"",""type"":""uint256""},{""internalType"":""uint256"",""name"":""characterIndex"",""type"":""uint256""}],""name"":""Sendscore"",""outputs"":[],""stateMutability"":""nonpayable"",""type"":""function""},{""inputs"":[{""internalType"":""uint256"",""name"":""_newCoinAmount"",""type"":""uint256""},{""internalType"":""uint256"",""name"":""_newCharacterIndex"",""type"":""uint256""},{""internalType"":""uint256"",""name"":""_exchangedAmount"",""type"":""uint256""}],""name"":""updateScoreByPlayerId"",""outputs"":[],""stateMutability"":""nonpayable"",""type"":""function""},{""inputs"":[{""internalType"":""uint256"",""name"":""_amount"",""type"":""uint256""}],""name"":""setExchangeAmount"",""outputs"":[],""stateMutability"":""nonpayable"",""type"":""function""},{""inputs"":[],""name"":""getAllData"",""outputs"":[{""internalType"":""uint256"",""name"":"""",""type"":""uint256""},{""components"":[{""internalType"":""uint256"",""name"":""playerId"",""type"":""uint256""},{""internalType"":""string"",""name"":""walletAddress"",""type"":""string""},{""internalType"":""uint256"",""name"":""coinAmount"",""type"":""uint256""},{""internalType"":""uint256"",""name"":""characterIndex"",""type"":""uint256""},{""internalType"":""uint256"",""name"":""exchangedAmount"",""type"":""uint256""},{""internalType"":""bool"",""name"":""exists"",""type"":""bool""}],""internalType"":""struct TestContract.Score[]"",""name"":"""",""type"":""tuple[]""}],""stateMutability"":""view"",""type"":""function"",""constant"":true},{""inputs"":[{""internalType"":""address"",""name"":""_wallet"",""type"":""address""}],""name"":""getPlayerData"",""outputs"":[{""internalType"":""uint256"",""name"":""playerId"",""type"":""uint256""},{""internalType"":""string"",""name"":""walletAddress"",""type"":""string""},{""internalType"":""uint256"",""name"":""coinAmount"",""type"":""uint256""},{""internalType"":""uint256"",""name"":""characterIndex"",""type"":""uint256""},{""internalType"":""uint256"",""name"":""exchangedAmount"",""type"":""uint256""},{""internalType"":""bool"",""name"":""exists"",""type"":""bool""}],""stateMutability"":""view"",""type"":""function"",""constant"":true},{""inputs"":[],""name"":""getClamps"",""outputs"":[{""internalType"":""uint256"",""name"":"""",""type"":""uint256""},{""internalType"":""uint256"",""name"":"""",""type"":""uint256""},{""internalType"":""uint256"",""name"":"""",""type"":""uint256""}],""stateMutability"":""view"",""type"":""function"",""constant"":true},{""inputs"":[{""internalType"":""uint256"",""name"":""_min"",""type"":""uint256""},{""internalType"":""uint256"",""name"":""_mid"",""type"":""uint256""},{""internalType"":""uint256"",""name"":""_max"",""type"":""uint256""}],""name"":""setModifiers"",""outputs"":[],""stateMutability"":""nonpayable"",""type"":""function""},{""inputs"":[{""internalType"":""uint256"",""name"":""currentLiquidity"",""type"":""uint256""}],""name"":""updateLiquidity"",""outputs"":[],""stateMutability"":""nonpayable"",""type"":""function""},{""inputs"":[],""name"":""getLiquidityLog"",""outputs"":[{""internalType"":""uint256[]"",""name"":"""",""type"":""uint256[]""}],""stateMutability"":""view"",""type"":""function"",""constant"":true}]";


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
            tokenInputField.onValueChanged.AddListener(ValidateInput);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void ValidateInput(string input)
    {
        if (int.TryParse(input, out int value))
        {
            if (value < 1 )
            {
                tokenInputField.text = "1";
            }
        }
        else if (!string.IsNullOrEmpty(input))
        {
            tokenInputField.text = ""; // Clear invalid input
        }
    }

    public async void SendScore(){

        string input = tokenInputField.text;
        Debug.Log("token " + input );

        if (uint.TryParse(input, out uint totalCoin))
        {
            var web3 = SDKManager.Instance.Web3;
            var contract = web3.Eth.GetContract( testabi, testcontractAddress );
            var SendScoreFunction = contract.GetFunction("Sendscore");

            try{

                var accountAddress = SDKManager.Instance.walletAddress;
                var result = await SendScoreFunction.SendTransactionAsync(
                    from: accountAddress,                     
                    gas: new HexBigInteger(900000),
                    value: null,
                    functionInput: new object[]
                    {
                        SDKManager.Instance.playerId,
                        SDKManager.Instance.walletAddress,      
                        totalCoin,
                        SDKManager.Instance.selectedCharacterIndex
                    }
                );

                SceneManager.LoadScene("Scene3");

            } 
            catch(RpcResponseException ex){
                Debug.LogError("Transaction failed: " + ex.Message);
            }
        }
        else
        {
            Debug.LogWarning("Invalid exchange value.");
        }
    }

    public async void SendExchangeAmount(){
        string input = tokenInputField.text;
        Debug.Log("token " + input );

        if (uint.TryParse(input, out uint exchangeCoin))
        {
            var web3 = SDKManager.Instance.Web3;
            var contract = web3.Eth.GetContract(testabi, testcontractAddress);
            var SendExchangeAmountFunction = contract.GetFunction("setExchangeAmount");

            try{
                var accountAddress = SDKManager.Instance.walletAddress;
                var result = await SendExchangeAmountFunction.SendTransactionAsync(
                    from: accountAddress,                     
                    gas: new HexBigInteger(900000),
                    value: null,
                    functionInput: new object[]
                    {
                        exchangeCoin 
                    }
                );
                SceneManager.LoadScene("Scene4");
            }
            catch(RpcResponseException ex){
                Debug.LogError("Transaction failed: " + ex.Message);
            }
        }
        else {
            Debug.LogWarning("Invalid exchange value.");
        }
    }


}


