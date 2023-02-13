using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Chroma {
public static class AdobeColorParser {
    /* Example XML from https://color.adobe.com/:
    <palette>
    <color name='Teals-1' rgb='0FC2C0' r='15' g='194' b='192' />
    <color name='Teals-2' rgb='0CABA8' r='12' g='171' b='168' />
    <color name='Teals-3' rgb='008F8C' r='0' g='143' b='140' />
    <color name='Teals-4' rgb='015958' r='1' g='89' b='88' />
    <color name='Teals-5' rgb='023535' r='2' g='53' b='53' />
    </palette>
    */
    private static Color[] XmlToColors(string paletteXml) {
        // Remove all new lines from the XML.
        paletteXml = Regex.Replace(paletteXml, @"\r\n?|\n", "");

        Regex regex = new Regex(@"<color name='(.*?)' rgb='(.*?)' r='(.*?)' g='(.*?)' b='(.*?)' />");
        MatchCollection matches = regex.Matches(paletteXml);

        if (matches.Count == 0) {
            Debug.LogError("Could not parse XML palette. Please make sure you are copying from https://color.adobe.com/.");
            return null;
        }

        var colors = new List<Color>();
        foreach (Match match in matches) {
            string name = match.Groups[1].Value;
            string rgb = match.Groups[2].Value;
            string r = match.Groups[3].Value;
            string g = match.Groups[4].Value;
            string b = match.Groups[5].Value;
            colors.Add(new Color(float.Parse(r) / 255, float.Parse(g) / 255, float.Parse(b) / 255));
        }

        return colors.ToArray();
    }

    public static Gradient ParseXml(string paletteXml) {
        var colors = XmlToColors(paletteXml);
        return GeneratorUtils.ColorsToGradient(colors);
    }
}
}