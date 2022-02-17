using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;

public class LastGameCanvasController : UdonSharpBehaviour
{
    [SerializeField] private Text winnerText;
    [SerializeField] private Text roundsSurvivedText;

    [HideInInspector][UdonSynced] public string winner;
    [HideInInspector][UdonSynced] public string rounds;

    public void UpdateDisplayText()
    {
        winnerText.text = $"Winner: {winner}";
        roundsSurvivedText.text = $"Rounds Survived: {rounds}";
    }

    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        if (Networking.IsOwner(gameObject))
            RequestSerialization();
    }
}
