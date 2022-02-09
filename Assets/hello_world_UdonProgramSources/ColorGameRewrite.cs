
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class ColorGameRewrite : UdonSharpBehaviour
{

    [SerializeField] private GameObject platformContainer;
    [SerializeField] private MeshRenderer[] colorRenderers;
    [SerializeField] private Transform startPoint;

    [Header("Materials")]
    [SerializeField] private Material[] colorMaterials;
    [SerializeField] private Material[] displayColors;
    [SerializeField] private int colorRendererIndex = 1;

    private MeshRenderer[] _platforms;
    [UdonSynced()]
    private int[] _dataPlatformsColors;
    [UdonSynced()]
    private bool[] _dataPlatformInactive;
    [UdonSynced()]
    private int _dataCurrentColor;

    private VRCPlayerApi[] _players;
    private bool _isGameRunning;

    void Start()
    {
        _players = new VRCPlayerApi[16];
        _platforms = GetComponentsInChildren<MeshRenderer>();
        _dataPlatformInactive = new bool[_platforms.Length];
        _dataPlatformsColors = new int[_platforms.Length];

        ResetGame();
    }

    void ResetGame()
    {
        for (var i = 0; i < _platforms.Length; i++)
        {
            _dataPlatformInactive[i] = false;
            _dataPlatformsColors[i] = 0;
        }
    }

    void RandomizeCurrentColor()
    {
        _dataCurrentColor = UnityEngine.Random.Range(0, colorMaterials.Length);
    }

    void RandomizePlatformColors()
    {
        for (var i = 0; i < _dataPlatformsColors.Length; i++)
        {
            _dataPlatformsColors[i] = UnityEngine.Random.Range(0, colorMaterials.Length);
        }
    }

    void DisablePlatforms()
    {
        for (var i = 0; i < _platforms.Length; i++)
        {
            _platforms[i].gameObject.SetActive(_dataPlatformsColors[i] != _dataCurrentColor);
        }
    }

    void ResetPlatformStates()
    {
        for (var i = 0; i < _platforms.Length; i++)
        {
            _dataPlatformInactive[i] = false;
            _dataPlatformsColors[i] = 0;
        }
    }

    public override void OnDeserialization()
    {
        OnUpdatePlatforms();
        OnUpdateCurrentColorDisplays();
    }

    void OnUpdatePlatforms()
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

    void OnUpdateCurrentColorDisplays()
    {
        foreach (var colorRenderer in colorRenderers)
        {
            colorRenderer.materials[colorRendererIndex] = displayColors[_dataCurrentColor];
        }
    }
}
