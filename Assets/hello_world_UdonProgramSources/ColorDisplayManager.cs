using UdonSharp;
using UnityEngine;

public class ColorDisplayManager : UdonSharpBehaviour
{
    [UdonSynced] public int CurrentColorIndex;
    [SerializeField] private MeshRenderer[] colorRenderers;
    [SerializeField] private Material[] displayColors;
    [SerializeField] private int colorRendererIndex = 1;

    public override void OnPreSerialization()
    {
        OnUpdateCurrentColorDisplays();
    }

    public override void OnDeserialization()
    {
        OnUpdateCurrentColorDisplays();
    }

    private void OnUpdateCurrentColorDisplays()
    {
        foreach (var colorRenderer in colorRenderers)
        {
            colorRenderer.materials[colorRendererIndex] = displayColors[CurrentColorIndex];
        }
    }
}
