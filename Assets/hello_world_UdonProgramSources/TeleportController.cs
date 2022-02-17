
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;

public class TeleportController : UdonSharpBehaviour
{
    [SerializeField] private float timeoutLength = 5;
        
    [UdonSynced][HideInInspector] public int playerId = -1;
    [UdonSynced][HideInInspector] public Vector3 teleportDest;
    [UdonSynced][HideInInspector] public Quaternion rotation;

    private float _timeoutAt = -1;

    public void TeleportPlayer()
    {
        if (!Networking.IsOwner(Networking.LocalPlayer, gameObject)) return;
        if (playerId == -1) return;

        _timeoutAt = Time.time + timeoutLength;
        RequestSerialization();
    }

    public void PlayerTeleported()
    {
        if (!Networking.IsOwner(Networking.LocalPlayer, gameObject)) return;
        playerId = -1;
        _timeoutAt = -1;
        RequestSerialization();
    }

    public void Update()
    {
        if (playerId == -1) return;
        if (Math.Abs(_timeoutAt - (-1)) < 0.1) return;
        if (Time.time < _timeoutAt) return;

        playerId = -1;
        _timeoutAt = -1;
    }

    public override void OnDeserialization()
    {
        TeleportPlayerClient();
    }

    private void TeleportPlayerClient()
    {
        if (playerId == -1) return;
        if (playerId != Networking.LocalPlayer.playerId) return;
        
        var player = VRCPlayerApi.GetPlayerById(playerId);
        
        player.TeleportTo(
            teleportDest,
            rotation
        );

        SendCustomNetworkEvent(NetworkEventTarget.Owner, "PlayerTeleported");
    }
}
