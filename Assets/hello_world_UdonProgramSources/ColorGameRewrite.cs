using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;

public class ColorGameRewrite : UdonSharpBehaviour
{
    [Header("Game Properties")]
    [SerializeField] private int colorCount;
    [SerializeField] private int secondsBetweenRounds = 3;
    [SerializeField] private float maxSecondsBetweenHideFloor = 7.0f;
    [SerializeField] private float minSecondsBetweenHideFloor = 3.0f;
    [SerializeField] private float roundStepSize = 0.25f;
    [SerializeField] private int secondsBetweenRoundEnd = 5;

    [Header("Teleporting")]
    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform respawnPoint;

    [Header("Scripts")]    
    [SerializeField] private PlatformManager platformManager;
    [SerializeField] private ColorDisplayManager colorDisplayManager;
    [SerializeField] private LastGameCanvasController lastGameCanvasController;
    [SerializeField] private NetworkManager networkManager;
    [SerializeField] private TimerController timerController;

    // State Variables
    [UdonSynced] private int _roundCount;
    [UdonSynced] private bool _isPlaying;
    private VRCPlayerApi[] _players = new VRCPlayerApi[16];
    private int _dataCurrentColor;

    public void OnGameStart()
    {
        // block if not owner
        if (!Networking.IsOwner(Networking.LocalPlayer, gameObject)) return;
        
        // block if game is already running
        if (_isPlaying) return;
        
        Debug.Log("Starting Game");
        
        // set initial game state
        _isPlaying = true;
        _roundCount = 0;
        RequestSerialization();
        
        // setup player list
        _players = new VRCPlayerApi[16];
        VRCPlayerApi.GetPlayers(_players);
        foreach (var player in _players)
        {
            if (player == null) continue;
            player.SetPlayerTag("alive", "true");
        }
        
        // teleport all players to start
        SendCustomNetworkEvent(NetworkEventTarget.All, "TeleportAllPlayersToStart");
        
        // Start Round
        OnRoundStart();
    }

    public void OnGameEnd()
    {
        // block if not owner
        if (!Networking.IsOwner(Networking.LocalPlayer, gameObject)) return;
        
        Debug.Log("Ending Game");
        
        // reset game state to not playing
        _isPlaying = false;
        _roundCount = 0;

        RequestSerialization();
    }

    public void OnRoundStart()
    {
        // block if not owner
        if (!Networking.IsOwner(Networking.LocalPlayer, gameObject)) return;
        
        Debug.Log("Starting Round");
        
        // increase the round cound
        _roundCount++;
        
        // selected the the current color
        _dataCurrentColor = Random.Range(1, colorCount);
        colorDisplayManager.currentColorIndex = _dataCurrentColor;
        platformManager.currentColorIndex = _dataCurrentColor;

        // randomize the floor colors
        platformManager.RandomizeFloorColor();

        // sync the game state
        RequestSerialization();
        colorDisplayManager.SyncState();
        platformManager.SyncState();
        
        // wait to hide floor
        var waitTillHideFloor = Mathf.Ceil(
            Mathf.Clamp(
                maxSecondsBetweenHideFloor - (roundStepSize * _roundCount),
                minSecondsBetweenHideFloor,
                maxSecondsBetweenHideFloor
                ));
        timerController.timerMax = waitTillHideFloor;
        timerController.timerStart = Time.time;
        timerController.RequestSerialization();
        SendCustomEventDelayedSeconds("OnRoundFloorHide", waitTillHideFloor);
    }

    public void OnRoundFloorHide()
    {
        // block if not owner
        if (!Networking.IsOwner(Networking.LocalPlayer, gameObject)) return;
        
        Debug.Log("Hiding Floor");
        
        // set platforms to enabled or not
        platformManager.HideTilesByColor();
        platformManager.SyncState();

        // wait to finish round
        SendCustomEventDelayedSeconds("OnRoundFinished", secondsBetweenRoundEnd);
    }

    public void OnRoundFinished()
    {
        // block if not owner
        if (!Networking.IsOwner(Networking.LocalPlayer, gameObject)) return;
        
        Debug.Log("Round Finished");
        
        // reset selected color
        _dataCurrentColor = 0;
        colorDisplayManager.currentColorIndex = _dataCurrentColor;
        platformManager.currentColorIndex = _dataCurrentColor;
        
        // reset platforms to intial state
        platformManager.ResetToInitialState();
        
        // sync game state
        colorDisplayManager.SyncState();
        platformManager.SyncState();
        RequestSerialization();

        // find how many players are left
        var aliveCount = 0;
        VRCPlayerApi winner = null;
        foreach (var player in _players)
        {
            if (aliveCount > 1) break;
            if (player == null) continue;
            if (player.GetPlayerTag("alive") != "true") continue;
            
            winner = player;
            aliveCount++;
        }

        if (aliveCount == 0) {
            // TIE Game
            lastGameCanvasController.rounds = _roundCount.ToString();
            lastGameCanvasController.winner = "Tie";
            lastGameCanvasController.SyncState();
            OnGameEnd();
        } else if (aliveCount == 1) {
            // WINNER
            lastGameCanvasController.rounds = _roundCount.ToString();
            lastGameCanvasController.winner = winner.displayName;
            lastGameCanvasController.SyncState();

            for (var i = 0; i < networkManager.playerIds.Length; i++)
            {
                if (networkManager.playerIds[i] == winner.playerId)
                {
                    networkManager.clients[i].SendCustomNetworkEvent(NetworkEventTarget.Owner, "TeleportPlayerToSpawn");
                }
            }
            
            OnGameEnd();
        } else {
            SendCustomEventDelayedSeconds("OnRoundStart", secondsBetweenRounds);
        }
    }

    public void TeleportAllPlayersToStart()
    {
        Networking.LocalPlayer.TeleportTo(startPoint.position, startPoint.rotation);
    }

    public void TeleportAllPlayersToSpawn()
    {
        Networking.LocalPlayer.TeleportTo(respawnPoint.position, respawnPoint.rotation);
    }
    
    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        platformManager.SyncState();
        colorDisplayManager.SyncState();
        RequestSerialization();
    }
}
