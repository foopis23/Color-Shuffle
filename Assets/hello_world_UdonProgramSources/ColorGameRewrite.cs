using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;

public class ColorGameRewrite : UdonSharpBehaviour
{
    [Header("Game Properties")]
    [SerializeField] private int colorCount;
    [SerializeField] private int secondsBetweenRounds = 3;
    [SerializeField] private int secondsBetweenDisableFloor = 5;
    [SerializeField] private int secondsBetweenRoundEnd = 5;

    [Header("Teleport Points")]
    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform respawnPoint;
    
    [Header("Scripts")]    
    [SerializeField] private PlatformManager platformManager;
    [SerializeField] private ColorDisplayManager colorDisplayManager;

    [UdonSynced] private int lastRoundsSurvived = 0;
    [UdonSynced] private string lastWinnerName;
    [UdonSynced] private int roundCount = 0;
    [UdonSynced] private bool isPlaying = false;
    private VRCPlayerApi[] players = new VRCPlayerApi[16];
    
    private int _dataCurrentColor;

    public void OnGameStart()
    {
        if (!Networking.IsOwner(Networking.LocalPlayer, gameObject)) return;
        
        roundCount = 0;
        VRCPlayerApi.GetPlayers(players);
        
        foreach (var player in players)
        {
            if (player == null) continue;
            player.SetPlayerTag("alive", "true");
            player.TeleportTo(startPoint.position, startPoint.rotation);
        }

        OnRoundStart();
    }

    public void OnGameEnd()
    {
        if (!Networking.IsOwner(Networking.LocalPlayer, gameObject)) return;
        isPlaying = false;
        roundCount = 0;
    }

    public void OnRoundStart()
    {
        if (!Networking.IsOwner(Networking.LocalPlayer, gameObject)) return;
        if (isPlaying) return;
        isPlaying = true;
        
        _dataCurrentColor = Random.Range(0, colorCount);
        colorDisplayManager.CurrentColorIndex = _dataCurrentColor;
        platformManager.CurrentColorIndex = _dataCurrentColor;
        platformManager.RandomizeFloorColor();
        roundCount++;
        
        SendCustomEventDelayedSeconds("OnRoundFloorHide", secondsBetweenDisableFloor);
    }

    public void OnRoundFloorHide()
    {
        if (!Networking.IsOwner(Networking.LocalPlayer, gameObject)) return;
        platformManager.HideTilesByColor();
        
        SendCustomEventDelayedSeconds("OnRoundFloorHide", secondsBetweenRoundEnd);
    }

    public void OnRoundFinished()
    {
        if (!Networking.IsOwner(Networking.LocalPlayer, gameObject)) return;
        _dataCurrentColor = 0;
        colorDisplayManager.CurrentColorIndex = _dataCurrentColor;
        platformManager.CurrentColorIndex = _dataCurrentColor;
        platformManager.ResetToInitialState();

        var aliveCount = 0;
        VRCPlayerApi winner = null;
        
        foreach (var player in players)
        {
            if (aliveCount > 1) break;
            if (player == null) continue;
            if (player.GetPlayerTag("alive") != "true") continue;
            
            winner = player;
            aliveCount++;
        }

        switch (aliveCount)
        {
            case 0:
                // TIE Game
                lastWinnerName = "Tie";
                lastRoundsSurvived = roundCount;
                OnGameEnd();
                break;
            case 1:
                // WINNER
                lastWinnerName = winner.displayName;
                lastRoundsSurvived = roundCount;
                winner.TeleportTo(startPoint.position, startPoint.rotation);
                OnGameEnd();
                break;
            default:
                SendCustomEventDelayedSeconds("OnRoundStart", secondsBetweenRounds);
                break;
        }
    }
}
