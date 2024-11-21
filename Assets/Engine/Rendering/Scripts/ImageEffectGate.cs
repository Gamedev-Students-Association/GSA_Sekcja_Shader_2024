using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ImageEffectGate : MonoBehaviour
{
    [SerializeField]
    private Material EffectMat;

    void OnRenderImage(RenderTexture ScreenImage, RenderTexture Depth)
    {

        Graphics.Blit(ScreenImage, Depth, EffectMat);
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
