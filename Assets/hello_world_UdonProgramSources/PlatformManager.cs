using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

public class PlatformManager : UdonSharpBehaviour
{
    public GameObject platformContainer;
    public Material[] colorMaterials;

    private MeshRenderer[] _platforms;
    private int[] _dataPlatformsColors;
    private bool[] _dataPlatformInactive;
    [HideInInspector] public int currentColorIndex = 0;
    [SerializeField] private RandomSync randomSync;

    private void Start()
    {
        currentColorIndex = 0;
        _platforms = platformContainer.transform.GetComponentsInChildren<MeshRenderer>();
        _dataPlatformInactive = new bool[_platforms.Length];
        _dataPlatformsColors = new int[_platforms.Length];
        
        ResetToInitialState();
    }

    public void ResetToInitialState()
    {
        for (var i = 0; i < _platforms.Length; i++)
        {
            _dataPlatformInactive[i] = false;
            _dataPlatformsColors[i] = 0;
        }
        
        OnFloorStateUpdate();
    }

    public void RandomizeFloorColor()
    {
        for (var i = 0; i < _dataPlatformsColors.Length; i++)
        {
            _dataPlatformsColors[i] = randomSync.randomNumbers[randomSync.index];
            randomSync.index++;
            randomSync.index %= randomSync.randomNumbers.Length;
        }
        
        OnFloorStateUpdate();
    }

    public void HideTilesByColor()
    {
        for (var i = 0; i < _platforms.Length; i++)
        {
            _dataPlatformInactive[i] = _dataPlatformsColors[i] != currentColorIndex;
        }
        
        OnFloorStateUpdate();
    }
    
    private void OnFloorStateUpdate()
    {
        for (var i = 0; i < _platforms.Length; i++)
        {
            var platform = _platforms[i];
            var color = colorMaterials[_dataPlatformsColors[i]];
            var inactive = _dataPlatformInactive[i];
            
            platform.material = color;
            platform.gameObject.SetActive(!inactive);
        }
    }
}
