using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LogDisplayManager : MonoBehaviour
{
    public Transform tableParent;
    public GameObject rowPrefab;

    public void AddRowToTable( string playerId, double coins, double exchanged, double usd, double eth)
    {
        tableParent.gameObject.SetActive(true);
        GameObject row = Instantiate(rowPrefab, tableParent);
        row.SetActive(true);

        var text = row.GetComponent<TMP_Text>();
        text.text = $"{playerId} | {coins} | {exchanged} | {usd} | {eth}";
    }
}
