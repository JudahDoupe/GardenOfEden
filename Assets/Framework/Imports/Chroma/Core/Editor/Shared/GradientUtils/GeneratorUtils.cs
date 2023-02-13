using UnityEngine;
using UnityEngine.Networking;

namespace Chroma {
public static class GeneratorUtils {
    public static Gradient ColorsToGradient(Color[] colors) {
        if (colors == null) {
            return null;
        }

        var colorKeys = new GradientColorKey[colors.Length];
        for (int i = 0; i < colors.Length; i++) {
            colorKeys[i].color = colors[i];
            colorKeys[i].time = 1f / (colorKeys.Length * 2f) + i / (float)colorKeys.Length;
        }

        Gradient gradient = new Gradient {
            mode = GradientMode.Blend,
            colorKeys = colorKeys,
            alphaKeys = new GradientAlphaKey[2] { new GradientAlphaKey(1, 0), new GradientAlphaKey(1, 1) }
        };

        return gradient;
    }

    public static Color HexToColor(string hex) {
        byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
        byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
        byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
        return new Color32(r, g, b, 255);
    }

    public static string GetWebpage(string url) {
        UnityWebRequest request = UnityWebRequest.Get(url);
        request.SendWebRequest();

        while (!request.isDone) {
            if (request.result == UnityWebRequest.Result.ConnectionError) {
                Debug.LogError("Connection error: " + request.error);
                return null;
            }
        }

        string html = request.downloadHandler.text;

        // Remove new lines and tabs
        html = html.Replace("\n", "");

        return html;
    }

    public static Gradient RandomComplimentary() {
        var baseColor = Random.ColorHSV(0f, 1f, 0.5f, 1f, 0.5f, 1f, 1f, 1f);
        Color.RGBToHSV(baseColor, out var h, out var s, out var v);
        var complimentaryColor = Color.HSVToRGB((h + 0.5f) % 1f, s, v);
        var colors = new Color[] { baseColor, complimentaryColor };
        return ColorsToGradient(colors);
    }

    public static Gradient RandomShades() {
        var baseColor = Random.ColorHSV(0f, 1f, 0.5f, 1f, 1f, 1f, 1f, 1f);
        Color.RGBToHSV(baseColor, out var h, out var s, out _);
        const int numColors = 8;
        var colors = new Color[numColors];

        for (var i = 0; i < numColors; i++) {
            colors[i] = Color.HSVToRGB(h, s, (i + 1f) / numColors);
        }

        return ColorsToGradient(colors);
    }

    public static void DistributeEvenly(ref Gradient gradient) {
        var colorKeys = gradient.colorKeys;
        for (var i = 0; i < colorKeys.Length; i++) {
            colorKeys[i].time = gradient.mode == GradientMode.Fixed
                ? (i + 1f) / colorKeys.Length
                : 1f / (colorKeys.Length * 2f) + i / (float)colorKeys.Length;
        }

        gradient = new Gradient { colorKeys = colorKeys, alphaKeys = gradient.alphaKeys, mode = gradient.mode };
    }
}
}