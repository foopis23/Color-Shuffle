using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

public class NetworkManager : UdonSharpBehaviour
{
    public int[] playerIds;
    [SerializeField] public NetworkClient[] clients;
    
    void Start()
    {
        playerIds = new int[16];
        for (var i = 0; i < playerIds.Length; i++)
        {
            playerIds[i] = -1;
        }
    }

    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        if (!Networking.IsOwner(gameObject)) return;
        for (var i = 0; i < playerIds.Length; i++)
        {
            if (playerIds[i] != -1) continue;
            
            clients[i].gameObject.SetActive(true);
            playerIds[i] = player.playerId;
            Networking.SetOwner(player, clients[i].gameObject);
            break;
        }
    }

    public override void OnPlayerLeft(VRCPlayerApi player)
    {
        if (!Networking.IsOwner(gameObject)) return;
        for (var i = 0; i < playerIds.Length; i++)
        {
            if (playerIds[i] != player.playerId) continue;
            
            clients[i].gameObject.SetActive(false);
            playerIds[i] = -1;
            break;
        }
    }
}
