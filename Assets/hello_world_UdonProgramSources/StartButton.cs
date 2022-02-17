
using UdonSharp;
using UnityEngine;
using VRC.Udon.Common.Interfaces;

public class StartButton : UdonSharpBehaviour
{
    [SerializeField] private ColorGameRewrite gameController;

    public override void Interact()
    {
        gameController.SendCustomNetworkEvent(NetworkEventTarget.All, "OnGameStart");
    }
}
