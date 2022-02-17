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
    [SerializeField] private TimerController timerController;
    [SerializeField] private RandomSync randomSync;

    // State Variables
    [UdonSynced] private int _roundCount;
    [UdonSynced] private bool _isPlaying;
    private VRCPlayerApi[] _players = new VRCPlayerApi[16];
    private int _dataCurrentColor;

    public void OnGameStart()
    {
        // block if game is already running
        if (_isPlaying) return;

        Debug.Log("Starting Game");
        
        // set initial game state
        _isPlaying = true;
        _roundCount = 0;

        // setup player list
        SetAllPlayersAlive();

        // teleport all players to start
        Networking.LocalPlayer.TeleportTo(startPoint.position, startPoint.rotation);

        if (Networking.IsOwner(gameObject))
        {
            randomSync.index = Random.Range(0, randomSync.randomNumbers.Length);
            randomSync.RequestSerialization();
            SendCustomEventDelayedSeconds("OnRoundStartOwner", secondsBetweenRounds);
        }
    }

    public void OnRoundStartOwner()
    {
        if (Networking.IsOwner(gameObject))
            SendCustomNetworkEvent(NetworkEventTarget.All, "OnRoundStart");
    }

    public void OnRoundStart()
    {
        Debug.Log("Starting Round");
        
        // increase the round cound
        _roundCount++;
        
        // selected the the current color
        _dataCurrentColor = randomSync.randomNumbers[randomSync.index];
        randomSync.index++;
        randomSync.index %=  randomSync.randomNumbers.Length;
        
        colorDisplayManager.currentColorIndex = _dataCurrentColor;
        platformManager.currentColorIndex = _dataCurrentColor;

        colorDisplayManager.OnUpdateCurrentColorDisplays();

        // randomize the floor colors
        platformManager.RandomizeFloorColor();

        // wait to hide floor
        var waitTillHideFloor = Mathf.Ceil(
            Mathf.Clamp(
                maxSecondsBetweenHideFloor - (roundStepSize * _roundCount),
                minSecondsBetweenHideFloor,
                maxSecondsBetweenHideFloor
                ));
        timerController.timerMax = waitTillHideFloor;
        timerController.timerStart = Time.time;

        if (Networking.IsOwner(gameObject))
            SendCustomEventDelayedSeconds("OnRoundFloorHideOwner", waitTillHideFloor);
    }

    public void OnRoundFloorHideOwner()
    {
        if (Networking.IsOwner(gameObject))
            SendCustomNetworkEvent(NetworkEventTarget.All, "OnRoundFloorHide");
    }
    
    public void OnRoundFloorHide()
    {
        Debug.Log("Hiding Floor");
        
        // set platforms to enabled or not
        platformManager.HideTilesByColor();
        
        // wait to finish round
        if (Networking.IsOwner(gameObject))
            SendCustomEventDelayedSeconds("OnRoundFinishedOwner", secondsBetweenRoundEnd);
    }

    public void OnRoundFinishedOwner()
    {
        SendCustomNetworkEvent(NetworkEventTarget.All, "OnRoundFinished");
    }

    public void OnRoundFinished()
    {
        Debug.Log("Round Finished");
        
        // reset selected color
        _dataCurrentColor = 0;
        colorDisplayManager.currentColorIndex = _dataCurrentColor;
        platformManager.currentColorIndex = _dataCurrentColor;
        
        colorDisplayManager.OnUpdateCurrentColorDisplays();
        
        // reset platforms to intial state
        platformManager.ResetToInitialState();
        
        TeleportPlayerToSpawnIfDead();

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
            lastGameCanvasController.UpdateDisplayText();
            OnGameEnd();
        } else if (aliveCount == 1) {
            // WINNER
            lastGameCanvasController.rounds = _roundCount.ToString();
            lastGameCanvasController.winner = winner.displayName;
            lastGameCanvasController.UpdateDisplayText();
            winner.TeleportTo(respawnPoint.position, respawnPoint.rotation);

            OnGameEnd();
        } else
        {
            if (!Networking.IsOwner(gameObject)) return;
            randomSync.RequestSerialization();
            SendCustomEventDelayedSeconds("OnRoundStartOwner", secondsBetweenRounds);
        }
    }
    
    public void OnGameEnd()
    {
        Debug.Log("Ending Game");
        
        // reset game state to not playing
        _isPlaying = false;
        _roundCount = 0;
    }

    private void SetAllPlayersAlive()
    {
        // setup player list
        _players = new VRCPlayerApi[16];
        VRCPlayerApi.GetPlayers(_players);
        foreach (var player in _players)
        {
            if (player == null) continue;
            player.SetPlayerTag("alive", "true");
        }
    }

    private void TeleportPlayerToSpawnIfDead()
    {
        for (var i = 0; i < _players.Length; i++)
        {
            var player = _players[i];
            if (player == null || player.GetPlayerTag("alive") == "true") continue;

            _players[i] = null;
            
            if (player.isLocal)
            {
                Debug.Log(player.playerId);
                Debug.Log(player.GetPlayerTag("alive"));
                player.TeleportTo(respawnPoint.position, respawnPoint.rotation);
            }
        }
    }

    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        if (!Networking.IsOwner(gameObject)) return;
        RequestSerialization();
    }
}
