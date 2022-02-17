
using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class TimerController : UdonSharpBehaviour
{
    [SerializeField] private Color startColor;
    [SerializeField] private Color endColor;
    [SerializeField] private Text[] text;
    
    [HideInInspector][UdonSynced] public float timerMax;
    [HideInInspector][UdonSynced] public float timerStart;

    void Update()
    {
        var currentTime = Mathf.Ceil(Mathf.Clamp(timerMax - (Time.time - timerStart), 0, timerMax));
        var color = Color.Lerp(endColor, startColor, currentTime / timerMax);
        foreach (var text1 in text)
        {
            text1.text = currentTime.ToString();
            text1.color = color;
        }
    }
}
