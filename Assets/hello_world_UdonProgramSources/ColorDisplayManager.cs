using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

public class ColorDisplayManager : UdonSharpBehaviour
{
    [HideInInspector] public int currentColorIndex = 0;
    [SerializeField] private MeshRenderer[] colorRenderers;
    [SerializeField] private Material[] displayColors;
    [SerializeField] private Material sandstoneMaterial;

    public void Start()
    {
        currentColorIndex = 0;
    }

    public void OnUpdateCurrentColorDisplays()
    {
        for (var i = 0; i < colorRenderers.Length; i++)
        {
            colorRenderers[i].materials = new[] {sandstoneMaterial, displayColors[currentColorIndex]};
        }
    }
}
