using System.Text.RegularExpressions;
using UnityEngine;

namespace Chroma {
public static class ColorHuntParser {
    // Example URL: https://colorhunt.co/palette/3330e4f637ecfbb454faea48
    private static Color[] UrlToColors(string url) {
        var html = GeneratorUtils.GetWebpage(url);

        // Find the start of the colors
        // Example substring: "<title>Color Palette: #3330E4 #F637EC #FBB454 #FAEA48 - Color Hunt</title>"
        Regex regex =
            new Regex("<title>Color Palette: #([0-9A-F]+) #([0-9A-F]+) #([0-9A-F]+) #([0-9A-F]+) - Color Hunt</title>");
        Match match = regex.Match(html);
        if (!match.Success) {
            Debug.LogError("Could not find colors in HTML");
            return null;
        }

        var colors = new Color[4];
        for (int i = 0; i < 4; i++) {
            string hex = match.Groups[i + 1].Value;
            colors[i] = GeneratorUtils.HexToColor(hex);
        }

        return colors;
    }

    public static Gradient UrlToGradient(string url) {
        var colors = UrlToColors(url);
        return GeneratorUtils.ColorsToGradient(colors);
    }
}
}