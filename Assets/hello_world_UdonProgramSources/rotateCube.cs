
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class rotateCube : UdonSharpBehaviour
{
    void Start()
    {
        
    }
    
    void Update() {
        transform.Rotate(Vector3.left * Time.deltaTime);
    }
}
