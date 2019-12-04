using UnityEngine;

public class ComputeShaderUtils : MonoBehaviour
{
    public static int TextureSize = 512;
    public static int WorldSizeInMeters = 400;

    public static void ResetTexture(RenderTexture tex)
    {
        tex.Release();
        tex.enableRandomWrite = true;
        tex.Create();
    }

    public static Vector2 LocationToUv(Vector3 location)
    {
        var relativePosition = location - GameObject.FindGameObjectWithTag("Renderers").transform.position;
        var uvPos = relativePosition / (WorldSizeInMeters / 2);
        var uv = new Vector2(uvPos.x, uvPos.z);
        return (uv + new Vector2(1, 1)) / 2;
    }

    public static Vector2 LocationToXy(Vector3 location)
    {
        var uv = LocationToUv(location);
        return new Vector2(Mathf.FloorToInt(uv.x * 512), Mathf.FloorToInt(uv.y * 512));
    }
}

public static class RenderTextureExtentions
{
    public static Texture2D ToTexture2D(this RenderTexture rt)
    {
        RenderTexture currentRt = RenderTexture.active;
        Texture2D rtnTex = new Texture2D(ComputeShaderUtils.TextureSize, ComputeShaderUtils.TextureSize, TextureFormat.RGBAFloat, false);
        RenderTexture.active = rt;

        rtnTex.ReadPixels(new Rect(0, 0, ComputeShaderUtils.TextureSize, ComputeShaderUtils.TextureSize), 0, 0);
        rtnTex.Apply();

        RenderTexture.active = currentRt;

        return rtnTex;
    }
}