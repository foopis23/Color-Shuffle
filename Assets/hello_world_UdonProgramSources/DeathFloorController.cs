using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

public class DeathFloorController : UdonSharpBehaviour
{
    [SerializeField] private Transform respawnPoint;

    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        player.TeleportTo(respawnPoint.position, respawnPoint.rotation);
        
        if (!Networking.IsOwner(Networking.LocalPlayer, gameObject)) return;
        Debug.Log("tragic character death");
        player.SetPlayerTag("alive", "false");
    }
}
