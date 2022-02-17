using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

public class NetworkClient : UdonSharpBehaviour
{
    public Transform spawnPoint;

    public void TeleportPlayerToSpawn()
    {
        Networking.GetOwner(gameObject).TeleportTo(spawnPoint.position, spawnPoint.rotation);
    }
}
