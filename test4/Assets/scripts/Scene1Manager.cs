using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class AccountsData
{
    public string[] addresses;
}

public class Scene1Manager : MonoBehaviour
{
    public static Scene1Manager Instance { get; private set; }

    [SerializeField] private Transform playerButtonContainer;  // Container for buttons (GridLayout/VerticalLayout)
    [SerializeField] private GameObject playerButtonPrefab;    // Your playerButton prefab

    private List<string> allPlayerAddresses = new List<string>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // Load accounts from JSON
        LoadAccountsFromJSON();
    }

    /// <summary>
    /// Load all 100 player addresses from accounts.json
    /// </summary>
    private void LoadAccountsFromJSON()
    {
        try
        {
            string path = Path.Combine(Application.dataPath, "AccountData/accounts.json");
            if (!File.Exists(path))
            {
                Debug.LogError($"❌ accounts.json not found at path: {path}");
                return;
            }

            string jsonText = File.ReadAllText(path);
            AccountsData data = JsonUtility.FromJson<AccountsData>(jsonText);

            if (data == null || data.addresses == null || data.addresses.Length == 0)
            {
                Debug.LogError("❌ Failed to parse accounts.json or no addresses found");
                return;
            }

            allPlayerAddresses = new List<string>(data.addresses);
            Debug.Log($"✅ Loaded {allPlayerAddresses.Count} player addresses from {path}");

            CreatePlayerButtons();
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ Error loading accounts.json: {ex.Message}");
        }
    }


    /// <summary>
    /// Create a button for each player address
    /// </summary>
    private void CreatePlayerButtons()
    {
        // Clear existing buttons (if any)
        foreach (Transform child in playerButtonContainer)
        {
            Destroy(child.gameObject);
        }

        // Create button for each address
        for (int i = 0; i < allPlayerAddresses.Count; i++)
        {
            string address = allPlayerAddresses[i];

            // Instantiate prefab
            GameObject buttonObj = Instantiate(playerButtonPrefab, playerButtonContainer);
            buttonObj.name = $"PlayerButton_{i + 1}";

            // Get button component
            Button btn = buttonObj.GetComponent<Button>();
            if (btn != null)
            {
                // Create closure to capture correct index
                int playerIndex = i;
                btn.onClick.AddListener(() => OnPlayerButtonClicked(playerIndex, address));
            }

            // Update button text to show address
            TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            //if (buttonText != null)
            //{
            // Show short version: 0x1234...5678
            string shortAddress = address.Length > 14
                    ? address.Substring(0, 2) + "..." + address.Substring(address.Length - 2)
                    : address;
            buttonText.text = $"Player {i + 1}\n{shortAddress}";
            //}

            Debug.Log($"✅ Created button for Player {i + 1}: {address}");
        }

        Debug.Log($"✅ All {allPlayerAddresses.Count} player buttons created!");
    }

    /// <summary>
    /// Handle when a player button is clicked
    /// </summary>
    private void OnPlayerButtonClicked(int playerIndex, string address)
    {
        Debug.Log($"🎮 Player {playerIndex + 1} selected: {address}");

        AccountManager.Instance.SelectPlayer(playerIndex);
    }
}
