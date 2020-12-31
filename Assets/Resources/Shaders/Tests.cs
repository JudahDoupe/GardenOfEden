using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Tests : MonoBehaviour
{
    public bool Run = false;
    public int3[] input;
    public int3[] expectedOutput;
    public int3[] output;

    public GameObject Dst;
    public GameObject Src;

    void Update()
    {
        if (Run)
        {
            Run = false;

            var shader = Resources.Load<ComputeShader>("Shaders/CoordinateTransformsTests");
            ComputeBuffer buffer = new ComputeBuffer(input.Length, 4 * 3);
            buffer.SetData(input);
            int kernel = shader.FindKernel("Test");
            shader.SetBuffer(kernel, "dataBuffer", buffer);
            shader.Dispatch(kernel, input.Length, 1, 1);

            buffer.GetData(output);


            for (int i = 0; i < output.Length; i++)
            {
                if (!output[i].Equals(expectedOutput[i]))
                {
                    Debug.Log($"Input:    {input[i]} \nOutput:   {output[i]} \nExpected: {expectedOutput[i]}");
                }
                else
                {
                    var src = new Coordinate(0, 1000, 0);
                    src.xyw = output[i];
                    var dst = new Coordinate(0, 1000, 0);
                    dst.xyw = input[i];
                    var dstObj = Instantiate(Dst);
                    dstObj.transform.position = dst.xyz;
                    dstObj.name = $"DST {i}";
                    var secObj = Instantiate(Src);
                    secObj.transform.position = src.xyz;
                    secObj.name = $"SRC {i}";
                }
            }

            buffer.Release();
        }
    }
}
