using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

public class DeathFloorController : UdonSharpBehaviour
{
    [SerializeField] private Transform respawnPoint;

    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        Debug.Log(player.playerId);
        player.SetPlayerTag("alive", "false");
    }
}
