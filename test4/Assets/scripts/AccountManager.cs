using Nethereum.HdWallet;
using Nethereum.Web3.Accounts;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class AccountData
{
    public string[] addresses;
    public string[] privateKeys;
}

public class AccountManager : MonoBehaviour
{
    public static AccountManager Instance { get; private set; }

    private AccountData accountData;
    public List<string> playerAddresses = new List<string>();
    private Dictionary<string, string> accountPrivateKeys = new Dictionary<string, string>();

    private string ownerAddress = "0x4ab5e0d87b8f27036a472acae9d7fbd34af4f51c";
    public string ownerPrivateKey = "0xc6d26544620c8fc1df085f8d0828f0c166a2d9d321adf6bed372250169ca430a";
    public int currentPlayerIndex = 0;
    public string CurrentAddress => playerAddresses.Count > 0 ? playerAddresses[currentPlayerIndex] : "";
    public string CurrentPrivateKey => accountPrivateKeys.ContainsKey(CurrentAddress) ? accountPrivateKeys[CurrentAddress] : "";

    private string accountsFilePath;
    public bool isReady = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        string projectPath = Application.dataPath;
        string dataFolder = Path.Combine(projectPath, "AccountData");

        if (!Directory.Exists(dataFolder))
        {
            Directory.CreateDirectory(dataFolder);
            Debug.Log($"📁 Created directory: {dataFolder}");
        }

        accountsFilePath = Path.Combine(dataFolder, "accounts.json");
        Debug.Log($"📁 Accounts file path: {accountsFilePath}");
    }

    void Start()
    {
        LoadOrGenerateAccounts();
    }

    private void LoadOrGenerateAccounts()
    {
        if (File.Exists(accountsFilePath))
        {
            LoadAccounts();
        }
        else
        {
            Debug.LogWarning("⚠️ No accounts file found!");
            Debug.Log($"📁 Expected at: {accountsFilePath}");
        }
    }

    [ContextMenu("Generate 100 Accounts")]
    public void GenerateAccounts()
    {
        Debug.Log("🔄 Generating 100 accounts...");

        List<string> addresses = new List<string>();
        List<string> privateKeys = new List<string>();

        string seedPhrase = "mirror about purpose liberty because oyster finger ugly protect dose mention hen";

        try
        {
            var wallet = new Wallet(seedPhrase, "");

            for (int i = 0; i < 100; i++)
            {
                var account = wallet.GetAccount(i);
                addresses.Add(account.Address);
                privateKeys.Add(account.PrivateKey);

                if (i < 5)
                {
                    Debug.Log($"Account {i}: {account.Address}");
                }
            }

            accountData = new AccountData
            {
                addresses = addresses.ToArray(),
                privateKeys = privateKeys.ToArray()
            };

            string directory = Path.GetDirectoryName(accountsFilePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                Debug.Log($"📁 Created directory: {directory}");
            }

            string json = JsonConvert.SerializeObject(accountData, Formatting.Indented);
            File.WriteAllText(accountsFilePath, json);

            Debug.Log($"✅ Generated and saved 100 accounts to: {accountsFilePath}");

#if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh();
#endif

            LoadAccounts();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Failed to generate accounts: {ex.Message}");
            Debug.LogError($"Stack trace: {ex.StackTrace}");
        }
    }

    [ContextMenu("Fix & Clean JSON")]
    public void FixJsonFile()
    {
        Debug.Log("🔧 Attempting to fix JSON...");

        try
        {
            string json = File.ReadAllText(accountsFilePath);

            // Remove trailing commas (common issue)
            json = System.Text.RegularExpressions.Regex.Replace(json, @",(\s*[\]}])", "$1");

            // Try to parse and re-serialize (removes any corruption)
            accountData = JsonConvert.DeserializeObject<AccountData>(json);

            string cleanJson = JsonConvert.SerializeObject(accountData, Formatting.Indented);
            File.WriteAllText(accountsFilePath, cleanJson);

            Debug.Log("✅ JSON fixed and cleaned!");

#if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh();
#endif

            LoadAccounts();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Failed to fix JSON: {ex.Message}");
            Debug.LogError($"Detailed error: {ex.StackTrace}");
        }
    }

    private void LoadAccounts()
    {
        try
        {
            string json = File.ReadAllText(accountsFilePath);

            // ✅ NEW: Better error handling with detailed position info
            try
            {
                accountData = JsonConvert.DeserializeObject<AccountData>(json, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });
            }
            catch (JsonReaderException jre)
            {
                Debug.LogError($"❌ JSON Syntax Error at Line {jre.LineNumber}, Position {jre.LinePosition}");
                Debug.LogError($"Error: {jre.Message}");
                Debug.LogError("💡 Try: Right-click AccountManager → Fix & Clean JSON");
                return;
            }

            if (accountData == null || accountData.addresses == null || accountData.privateKeys == null)
            {
                Debug.LogError("❌ JSON structure invalid: missing 'addresses' or 'privateKeys'");
                return;
            }

            // ✅ NEW: Validate matching arrays
            if (accountData.addresses.Length != accountData.privateKeys.Length)
            {
                Debug.LogError($"❌ Mismatch! Addresses: {accountData.addresses.Length}, PrivateKeys: {accountData.privateKeys.Length}");
                return;
            }

            playerAddresses.Clear();
            accountPrivateKeys.Clear();

            // adding owneraddress as the first address
            //playerAddresses.Add(ownerAddress);
            //accountPrivateKeys[ownerAddress] = ownerPrivateKey;

            for (int i = 0; i < accountData.addresses.Length; i++)
            {
                string address = accountData.addresses[i].ToLowerInvariant();
                playerAddresses.Add(address);
                accountPrivateKeys[address] = accountData.privateKeys[i];
            }

            Debug.Log($"✅ Loaded {playerAddresses.Count} accounts");

            currentPlayerIndex = 0;
            isReady = true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Failed to load accounts: {ex.Message}");
        }
    }

    public void SelectPlayer(int playerIndex)
    {
        if (playerIndex < 0 || playerIndex >= playerAddresses.Count)
        {
            Debug.LogError($"❌ Invalid player index: {playerIndex}");
            return;
        }

        currentPlayerIndex = playerIndex;
        SceneManager.LoadScene("Scene2");
        SDKManager.Instance.UpdateWeb3WithCurrentPlayer();
        Debug.Log($"✅ Selected Player {playerIndex + 1}/{playerAddresses.Count}: {CurrentAddress.Substring(0, 10)}...");
    }

    public void NextPlayer()
    {
        int nextIndex = (currentPlayerIndex + 1) % playerAddresses.Count;
        SelectPlayer(nextIndex);
    }

    public void PreviousPlayer()
    {
        int prevIndex = (currentPlayerIndex - 1 + playerAddresses.Count) % playerAddresses.Count;
        SelectPlayer(prevIndex);
    }
}
