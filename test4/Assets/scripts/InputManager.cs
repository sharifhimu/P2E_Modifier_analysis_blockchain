using UnityEngine;
using TMPro;
using Nethereum.Hex.HexTypes;
using UnityEngine.SceneManagement;
using Nethereum.JsonRpc.Client;


public class InputManager : MonoBehaviour
{
    public TMP_InputField tokenInputField;

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
            var contract = web3.Eth.GetContract(SDKManager.Instance.abi, SDKManager.Instance.contractAddress);
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
            var contract = web3.Eth.GetContract(SDKManager.Instance.abi, SDKManager.Instance.contractAddress);
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


