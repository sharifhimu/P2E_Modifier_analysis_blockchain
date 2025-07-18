using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System;
using TMPro;
using Nethereum.JsonRpc.Client;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;



public class Log : MonoBehaviour
{
    
    public TextMeshProUGUI LogText; 

    public Transform tableParent;
    public GameObject rowPrefab;

    public GameObject closeBtn;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public async void CalculateModifierBasedpriceAndLog(){

        double marketPrice = (double)SDKManager.Instance.marketPrice;
        double modifier = SDKManager.Instance.modifier1;

        double totalTokenDbl = (double)SDKManager.Instance.TotalToken;
        double exchangeableTokenDbl = (double) SDKManager.Instance.TotalExchangeableToken;

        double effectivePrice = marketPrice * modifier;
        double priceUsd = (double)SDKManager.Instance.priceUsd;

        double usdReceivedDbl = exchangeableTokenDbl * priceUsd;
        double ethReceivedDbl = exchangeableTokenDbl * effectivePrice;

        LogText.text = $" market Price: {marketPrice}, effective Price: {effectivePrice},";
        
        await LogExchange( effectivePrice, priceUsd );
    }

    public async Task LogExchange( double effectivePrice, double priceUsd )
    {


        string filePath = Application.persistentDataPath + "/exchange_log.csv";
        Debug.Log($"path to save file: {Application.persistentDataPath}");
        string date = DateTime.Now.ToString("yyyy-MM-dd");
        
        // If file doesn't exist, create with header
        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath, "Date,PlayerID,CoinAmount,ExchangeTokens,modifier1,modifier2,modifier3,modifier,GameUSD,GameETH\n");
        }
        
        var web3 = SDKManager.Instance.Web3;
        var contract = web3.Eth.GetContract(SDKManager.Instance.abi, SDKManager.Instance.contractAddress);
        var GetDataFunction = contract.GetFunction("getAllData");

        try{

            var result = await GetDataFunction.CallDeserializingToObjectAsync<GetAllDataOutputDTO>();
            
            foreach (var score in result.Scores)
            {
                // Debug.Log($"PlayerID: {score.PlayerId}, Wallet: {score.WalletAddress}, Coins: {score.CoinAmount}, exchange: { score.exchangedAmount }, Character: {score.CharacterIndex}, Exists: {score.Exists} ");

                string playerIdStr = (score.PlayerId).ToString();
                double exchangeAmntDouble = (double)score.exchangedAmount;
                double tokenUsd = exchangeAmntDouble*priceUsd;
                double tokenEth = exchangeAmntDouble*effectivePrice;
                double coinAmntDbl = (double)score.CoinAmount;

                string newLine = $"{date},{score.PlayerId},{score.CoinAmount},{score.exchangedAmount},{SDKManager.Instance.modifierOne},{SDKManager.Instance.modifierTwo},{SDKManager.Instance.modifierThree},{SDKManager.Instance.modifier1},{tokenUsd},{tokenEth}";

                // table code
                AddRowToTable(score.PlayerId.ToString(), coinAmntDbl, exchangeAmntDouble, tokenUsd, tokenEth);
                

                // Read all lines
                List<string> lines = File.ReadAllLines(filePath).ToList();

                // Check for existing line with same date and playerId
                int existingIndex = lines.FindIndex(line =>
                {
                    string[] parts = line.Split(',');

                    if (parts.Length < 2)
                        return false;

                    string partDateRaw = parts[0].Trim();
                    string partPlayerId = parts[1].Trim();

                    DateTime partDateParsed;
                    DateTime inputDateParsed;

                    bool isPartDateValid = DateTime.TryParse(partDateRaw, out partDateParsed);
                    bool isInputDateValid = DateTime.TryParse(date, out inputDateParsed);

                    // Debug.Log($"Comparing CSV date '{partDateParsed.ToString("yyyy-MM-dd")}' to '{inputDateParsed.ToString("yyyy-MM-dd")}' and playerID '{partPlayerId}' to '{playerIdStr}'");

                    return isPartDateValid && isInputDateValid
                        && partDateParsed.Date == inputDateParsed.Date
                        && partPlayerId == playerIdStr.Trim();
                });
                

                // Debug.Log("existingIndex " + existingIndex );

                if (existingIndex >= 0)
                {
                    lines[existingIndex] = newLine; // Replace
                }
                else
                {
                    lines.Add(newLine); // Add new
                }

                File.WriteAllLines(filePath, lines);
                Debug.Log("Logged/Updated: " + newLine);
                
            }

        }
        catch(RpcResponseException ex){
            Debug.LogError("logging/fetching all data failed: " + ex.Message);
        }


    }



    public void AddRowToTable( string playerId, double coins, double exchanged, double usd, double eth)
    {
        tableParent.gameObject.SetActive(true);
        GameObject row = Instantiate(rowPrefab, tableParent);
        row.SetActive(true);
        closeBtn.SetActive(true);
        var text = row.GetComponent<TMP_Text>();
        text.text = $"{playerId} | {coins} | {exchanged} | {usd} | {eth}";
    }

    public void CloseBtnClick(){
        tableParent.gameObject.SetActive(false);
        closeBtn.SetActive(false);
    }
    
    public void startAgain(){
        SceneManager.LoadScene("Scene1");
    }



}
