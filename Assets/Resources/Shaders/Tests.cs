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
            }

            buffer.Release();
        }
    }
}
