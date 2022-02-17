using UdonSharp;
using UnityEngine;
using UnityEngine.UI;

public class LastGameCanvasController : UdonSharpBehaviour
{
    [SerializeField] private Text winnerText;
    [SerializeField] private Text roundsSurvivedText;

    [HideInInspector][UdonSynced] public string winner;
    [HideInInspector][UdonSynced] public string rounds;

    public override void OnDeserialization()
    {
        UpdateDisplayText();
    }
    
    public void SyncState()
    {
        RequestSerialization();
        UpdateDisplayText();
    }

    private void UpdateDisplayText()
    {
        winnerText.text = $"Winner: {winner}";
        roundsSurvivedText.text = $"Rounds Survived: {rounds}";
    }
}
