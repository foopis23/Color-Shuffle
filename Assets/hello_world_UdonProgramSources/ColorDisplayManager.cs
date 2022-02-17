using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

public class ColorDisplayManager : UdonSharpBehaviour
{
    [UdonSynced] [HideInInspector] public int currentColorIndex = 0;
    [SerializeField] private MeshRenderer[] colorRenderers;
    [SerializeField] private Material[] displayColors;
    [SerializeField] private Material sandstoneMaterial;

    public void Start()
    {
        currentColorIndex = 0;
    }

    public override void OnDeserialization()
    {
        OnUpdateCurrentColorDisplays();
    }
    
    public void SyncState()
    {
        if (!Networking.IsOwner(Networking.LocalPlayer, gameObject)) return;
        
        RequestSerialization();
        OnUpdateCurrentColorDisplays();
    }

    public void OnUpdateCurrentColorDisplays()
    {
        for (var i = 0; i < colorRenderers.Length; i++)
        {
            colorRenderers[i].materials = new[] {sandstoneMaterial, displayColors[currentColorIndex]};
        }
    }
}
