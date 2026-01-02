using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SDKManager : MonoBehaviour
{
    public static SDKManager Instance { get; private set; }
    public Web3 Web3 { get; private set; }
    public Web3 Web3Owner { get; private set; }

    [NonSerialized] public string rpcUrl = "https://ethereum-sepolia-rpc.publicnode.com";

    [HideInInspector] public string ownerAddress;
    [HideInInspector] public string walletAddress;

    [HideInInspector] public List<string> playerAddresses;

    public int selectedCharacterIndex = 0;
    public BigInteger totalCoin = 0;
    public int playerId = 1;

    public MerkleTree CurrentMerkleTree { get; set; }
    public List<byte[]> CurrentLeaves { get; set; }
    public BigInteger CurrentEpochId { get; set; }
    public Dictionary<string, BigInteger> playerAllowences = new();

    public GameObject buildMerkleTree;

    public bool localProjectRunning = true;

    async void Start()
    {
        Application.runInBackground = true;
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        DontDestroyOnLoad(buildMerkleTree.gameObject);

        // ✅ Wait for AccountManager to load accounts
        Debug.Log("⏳ Waiting for AccountManager...");
        while (AccountManager.Instance == null || !AccountManager.Instance.isReady)
        {
            await Task.Delay(100);
        }

        Debug.Log("✅ AccountManager ready!");

        // ✅ Get all player addresses from AccountManager
        playerAddresses = AccountManager.Instance.playerAddresses;

        // ✅ Initialize OWNER Web3 once (for Merkle root, admin ops)
        var ownerAccount = new Account(AccountManager.Instance.ownerPrivateKey);
        Web3Owner = new Web3(ownerAccount, rpcUrl);
        ownerAddress = ownerAccount.Address;
        Debug.Log($"✅ Web3Owner initialized for owner: {ownerAddress}");

        // ✅ Initialize Web3 with first player
        UpdateWeb3WithCurrentPlayer();

        Debug.Log($"✅ Loaded {playerAddresses.Count} players");

        // ✅ Start epoch loop (FIRE AND FORGET - doesn't block Start())
        _ = StartEpochLoopAsync(playerAddresses);
    }

    public void UpdateWeb3WithCurrentPlayer()
    {
        string address = AccountManager.Instance.CurrentAddress;
        string privateKey = AccountManager.Instance.CurrentPrivateKey;

        if (string.IsNullOrEmpty(address) || string.IsNullOrEmpty(privateKey))
        {
            Debug.LogError("❌ No account selected in AccountManager!");
            return;
        }

        var account = new Account(privateKey);
        Web3 = new Web3(account, rpcUrl);
        walletAddress = address;

        Debug.Log($"✅ Web3 updated for: {address}...");
    }

    private async Task StartEpochLoopAsync(List<string> playerAddresses)
    {
        try { 
        
            var pairContract = Web3Owner.Eth.GetContract(ABIManager.tokenpairABI, ABIManager.tokenpairAddress);
            var epochDurationFn = pairContract.GetFunction("epochDuration");

            BigInteger durationSec = await epochDurationFn.CallAsync<BigInteger>();
            double durationFloat = (double)durationSec;
            Debug.Log("durationFloat " + durationFloat);

            BuildMerkleTree buildMerkleTreeInstance = buildMerkleTree.GetComponent<BuildMerkleTree>();

                Debug.Log($" localProjectRunning: {localProjectRunning} ");
            if (localProjectRunning) {
                while (true)  // ← INFINITE LOOP
                {
                    // ✅ This runs AFTER every delay
                    Debug.Log($"⚡ EPOCH TRIGGERED! Updating Merkle Root...");
                    await buildMerkleTreeInstance.CheckAndSaveBalancesAsync(playerAddresses);

                    Debug.Log($"⏳ Waiting {durationFloat} seconds for next epoch...");
                    await Task.Delay(System.TimeSpan.FromSeconds(durationFloat));

                }
            }
            else
            {
                SceneManager.LoadScene("Scene1");
            }

        }
        catch (System.Exception ex)
        {
            Debug.LogError("error while looping epoch: " + ex.Message);
        }
    }

}
