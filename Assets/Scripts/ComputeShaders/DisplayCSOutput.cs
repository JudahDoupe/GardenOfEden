using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayCSOutput : MonoBehaviour
{
    public ComputeShader shader;

    void Start()
    {
        RunShader();
    }

    public void RunShader()
    {
        int kernelId = shader.FindKernel("CSMain");

        RenderTexture tex = new RenderTexture(256, 256, 24);
        tex.enableRandomWrite = true;
        tex.Create();

        shader.SetTexture(kernelId, "Result", tex);
        shader.Dispatch(kernelId, 256 / 8, 256 / 8, 1);

        transform.GetComponent<Renderer>().material.mainTexture = tex;
    }
}
