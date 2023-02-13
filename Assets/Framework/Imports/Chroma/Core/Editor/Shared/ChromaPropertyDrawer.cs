using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Chroma;
using JetBrains.Annotations;
using UnityEngine;
using Color = UnityEngine.Color;

namespace UnityEditor {
public static class ChromaPropertyDrawer {
    public static void DrawProperties(MaterialProperty[] properties, MaterialEditor editor, ChromaDrawers drawers) {
        var foldoutCount = 0;

        var material = editor.target as Material;
        if (material == null) {
            EditorGUILayout.HelpBox("[Chroma] Material is null", MessageType.Error);
            return;
        }

        foreach (var property in properties) {
            var tooltip = GetTooltip(property, properties);
            if (tooltip == null) continue;
            var indent = EditorGUI.indentLevel;
            var drawn = DrawProperty(property, editor, material, tooltip, drawers, ref foldoutCount);
            if (!drawn) {
                var trimmedName = RemoveEverythingInBrackets(property.displayName);
                var guiContent = new GUIContent(trimmedName, tooltip);
                editor.ShaderProperty(property, guiContent);
            }

            EditorGUI.indentLevel = indent;
        }
    }

    [MustUseReturnValue]
    public static bool DrawProperty(MaterialProperty property, MaterialEditor editor, Material material, string tooltip,
                                    ChromaDrawers drawers, ref int foldoutCount) {
        bool hideInInspector = (property.flags & MaterialProperty.PropFlags.HideInInspector) != 0;
        if (hideInInspector) {
            return true;
        }

        var trimmedName = RemoveEverythingInBrackets(property.displayName);

        // Handle tabs.
        var indent = NumTabs(property.displayName);

        // Handle spaces.
        if (HasSpaceAttribute(property.displayName)) {
            var parameters = ExtractAttributeParameters(property.displayName, "s")
                .Union(ExtractAttributeParameters(property.displayName, "space")).ToArray();
            if (parameters.Length > 0) {
                bool parsed = float.TryParse(parameters[0], NumberStyles.Float, CultureInfo.InvariantCulture,
                                             out var space);
                if (!parsed) {
                    space = 10;
                    var message = $"Could not parse space attribute: `<i>{parameters[0]}</i>`. " +
                                  $"Please use a float value. Defaulting to {space}.";
                    Log.M(message);
                }

                EditorGUILayout.Space(space);
            } else {
                EditorGUILayout.Space();
            }
        }

        // Handle foldouts.
        {
            const string foldoutMetaKey = "[Chroma]-foldout-key";
            if (HasFoldoutAttribute(property.displayName)) {
                foldoutCount = GetFoldoutCount(property.displayName);

                var foldoutKey = $"[Chroma]-expanded-{property.name}";
                EditorPrefs.SetString(foldoutMetaKey, foldoutKey);
                var expanded = EditorPrefs.GetBool(foldoutKey, false);
                var paddedName = $" {trimmedName}";
                expanded = EditorGUILayout.Foldout(expanded, new GUIContent(paddedName, tooltip));
                EditorPrefs.SetBool(foldoutKey, expanded);

                return true;
            }

            if (!IsPureAttribute(property.displayName)) {
                --foldoutCount;
            }

            if (foldoutCount >= 0) {
                var foldoutKey = EditorPrefs.GetString(foldoutMetaKey, String.Empty);
                var expanded = EditorPrefs.GetBool(foldoutKey, false);
                if (expanded) {
                    ++indent;
                } else {
                    return true;
                }
            }
        }

        EditorGUI.indentLevel += indent;

        // Handle headers.
        if (HasHeaderAttribute(property.displayName)) {
            EditorGUILayout.Space();
            var parameter = ExtractAttributeParameter(property.displayName, "h") ??
                            ExtractAttributeParameter(property.displayName, "header");
            var style = new GUIStyle(EditorStyles.boldLabel) {
                fontSize = int.TryParse(parameter, out var size) ? size : 13, stretchHeight = true
            };
            EditorGUILayout.LabelField(new GUIContent(trimmedName, tooltip), style);
            EditorGUI.indentLevel -= indent;
            return true;
        }

        // Handle notes.
        if (HasNoteAttribute(property.displayName)) {
            var parameter = ExtractAttributeParameter(property.displayName, "n") ??
                            ExtractAttributeParameter(property.displayName, "note");
            var style = new GUIStyle(EditorStyles.helpBox) {
                fontSize = int.TryParse(parameter, out var size) ? size : 12, stretchHeight = true, wordWrap = true
            };
            EditorGUILayout.LabelField(new GUIContent(trimmedName, tooltip), style);
            EditorGUI.indentLevel -= indent;
            return true;
        }

        // Handle horizontal lines.
        if (HasLineAttribute(property.displayName)) {
            var parameter = ExtractAttributeParameter(property.displayName, "l") ??
                            ExtractAttributeParameter(property.displayName, "line");
            var color = Color.gray;
            if (!string.IsNullOrEmpty(parameter)) {
                try {
#if UNITY_2021_2_OR_NEWER
                    var c = ColorTranslator.FromHtml(parameter);
                    color = new Color(c.R / 255f, c.G / 255f, c.B / 255f, c.A / 255f);
#else
                    var parsed = ColorUtility.TryParseHtmlString(parameter, out var c);
                    if (!parsed) {
                        Log.M($"Could not parse color: `<i>{parameter}</i>`. Please use a valid HTML color. Defaulting to gray.");
                    }

                    color = c;
#endif
                }
                catch (Exception e) {
                    Log.M($"Could not parse the line color. Reason:\n{e}.");
                }
            }

            HorizontalLine.Draw(color);
        }

        // Handle "Show if" attributes.
        if (HasShowIfAttribute(property.displayName)) {
            var parameters = ExtractAttributeParameters(property.displayName, "si")
                .Union(ExtractAttributeParameters(property.displayName, "showif")).ToArray();
            if (parameters.Length > 0) {
                bool Predicate(string parameter) {
                    var p = parameter.ToUpper().Replace(" ", "");

                    // - Using `material.shaderKeywords` instead of `material.IsKeywordEnabled` because the latter
                    //   requires `#pragma shader_feature` in code shaders.
                    // - Code shaders add a `_ON` postfix to the keyword.
                    // - Adding `_` prefix just in case the user provided display name instead of reference.

                    // Check toggle.
                    if (Array.Exists(material.shaderKeywords,
                                     keyword => keyword == p || keyword == $"_{p}" || keyword == $"{p}_ON")) {
                        return true;
                    }

                    // Check enum.
                    var split = p.Split('=');
                    if (split.Length == 2) {
                        var k = split[0];
                        var v = split[1];
                        if (Array.Exists(material.shaderKeywords, keyword => keyword == $"{k}_{v}")) {
                            return true;
                        }
                    }

                    return false;
                }

                var allExist = parameters.All(Predicate);
                if (!allExist) {
                    EditorGUI.indentLevel -= indent;
                    return true;
                }
            } else {
                Log.M($"Could not parse the show if attribute. Please use the following format: " +
                      $"<i>[ShowIf(PropertyName)]</i>.");
            }
        }

        if (HasGradientAttribute(property)) {
            EditorGUILayout.Space(18);
            int resolution = 256;
            bool hdr = false;
            string[] parameters = ExtractAttributeParameters(property.displayName, "Gradient");
            foreach (var parameter in parameters) {
                if (int.TryParse(parameter, out var r)) {
                    resolution = r;
                } else if (string.Equals(parameter.ToLower(), "hdr")) {
                    hdr = true;
                }
            }

            string key = string.Join("_", parameters);
            if (!drawers.gradient.TryGetValue(key, out GradientDrawer drawer)) {
                drawer = new GradientDrawer(resolution, hdr ? "hdr" : "");
                drawers.gradient.Add(key, drawer);
            }

            drawer.OnGUI(Rect.zero, property, trimmedName, editor, tooltip);
        } else if (HasCurveAttribute(property)) {
            EditorGUILayout.Space(18);
            drawers.curve.OnGUI(Rect.zero, property, trimmedName, editor, tooltip);
        } else if (property.type == MaterialProperty.PropType.Vector &&
                   property.displayName.ToLower().Contains("[vector2]")) {
            EditorGUILayout.Space(18);
            drawers.vector2.OnGUI(Rect.zero, property, trimmedName, editor, tooltip);
        } else if (property.type == MaterialProperty.PropType.Vector &&
                   property.displayName.ToLower().Contains("[vector3]")) {
            EditorGUILayout.Space(18);
            drawers.vector2.OnGUI(Rect.zero, property, trimmedName, editor, tooltip);
        } else if (HasMinMaxAttribute(property)) {
            EditorGUILayout.Space(18);
            Vector2 range = new Vector2(0, 1);
            string[] parameters = ExtractAttributeParameters(property.displayName, "MinMax");
            if (parameters.Length == 0) {
                // No parameters, use default range.
            } else if (parameters.Length == 1) {
                range = new Vector2(0, float.Parse(parameters[0]));
            } else if (parameters.Length == 2) {
                range = new Vector2(float.Parse(parameters[0]), float.Parse(parameters[1]));
            } else {
                var message = $"MinMax attribute {trimmedName} has invalid parameters. Expected up to 2 " +
                              $"parameters, but got {parameters.Length}. Material: {editor.target.name}, " +
                              $"Property: {property.displayName}, Parameters: {string.Join(", ", parameters)}";
                Log.M(message);
            }

            if (!drawers.minMax.TryGetValue(range, out MinMaxDrawer drawer)) {
                drawer = new MinMaxDrawer(range, range);
                drawers.minMax.Add(range, drawer);
            }

            drawer.OnGUI(Rect.zero, property, trimmedName, editor);
        } else if (property.type == MaterialProperty.PropType.Texture && HasMiniAttribute(property.displayName)) {
            EditorGUILayout.Space();
            var guiContent = new GUIContent(trimmedName, tooltip);
            editor.TexturePropertySingleLine(guiContent, property);
        } else {
            if (!IsPureAttribute(property.displayName)) {
                // We're not drawing this property, but we still need to remove attributes form it. For example, if
                // a float property has a [Tab] attribute we don't want that to be displayed in the Material inspector.
                var displayNameField = property.GetType()
                    .GetField("m_DisplayName", BindingFlags.NonPublic | BindingFlags.Instance);
                displayNameField?.SetValue(property, trimmedName);

                return false;
            }
        }

        EditorGUI.indentLevel -= indent;
        return true;
    }

    public static string GetTooltip(MaterialProperty property, MaterialProperty[] properties) {
        var tooltip = "";

        // Do not display the tooltip field itself.
        if (HasTooltipAttribute(property.displayName)) {
            return null;
        }

        // [Tooltip] This is a sequential tooltip.
        var propertyIndex = Array.IndexOf(properties, property);
        var regex = new Regex(@"\[(?<attribute>tooltip|tt)\](?<tooltip>.*)", RegexOptions.IgnoreCase);
        if (properties.Length > propertyIndex + 1 && regex.IsMatch(properties[propertyIndex + 1].displayName)) {
            var match = regex.Match(properties[propertyIndex + 1].displayName);
            tooltip = match.Groups["tooltip"].Value.Trim();
        }

        // [Tooltip(Display Name)] This is a referenced tooltip.
        regex = new Regex(@"\[(?<attribute>tooltip|tt)\((?<displayName>.*)\)\](?<tooltip>.*)", RegexOptions.IgnoreCase);
        foreach (var p in properties) {
            var match = regex.Match(p.displayName);
            if (match.Success && match.Groups["displayName"].Value.Trim() == property.displayName) {
                tooltip = match.Groups["tooltip"].Value.Trim();
                break;
            }
        }

        return tooltip;
    }

    public static bool HasTooltipAttribute(string displayName) {
        var s = displayName.ToLower();
        return s.Contains("[tooltip") || s.Contains("[tt]") || s.Contains("[tt(");
    }

    public static bool HasAnyAttributes(string displayName) {
        return displayName.Contains('[') && displayName.Contains(']');
    }

    public static bool HasHeaderAttribute(string displayName) {
        var s = displayName.ToLower();
        return s.Contains("[header") || s.Contains("[h]") || s.Contains("[h(");
    }

    public static bool HasNoteAttribute(string displayName) {
        var s = displayName.ToLower();
        return s.Contains("[note") || s.Contains("[n(") || s.Contains("[n]");
    }

    public static bool HasSpaceAttribute(string displayName) {
        var s = displayName.ToLower();
        return s.Contains("[space") || s.Contains("[s]") || s.Contains("[s(");
    }

    public static bool HasLineAttribute(string displayName) {
        var s = displayName.ToLower();
        return s.Contains("[line") || s.Contains("[l]") || s.Contains("[l(");
    }

    private static bool HasShowIfAttribute(string displayName) {
        var s = displayName.ToLower();
        return s.Contains("[showif") || s.Contains("[si]") || s.Contains("[si(");
    }

    public static bool HasFoldoutAttribute(string displayName) {
        var s = displayName.ToLower();
        return s.Contains("[foldout") || s.Contains("[f");
    }

    public static int GetFoldoutCount(string displayName) {
        var s = displayName.ToLower();
        var parameters = ExtractAttributeParameters(s, "foldout").Union(ExtractAttributeParameters(s, "f")).ToArray();
        if (parameters.Length == 0) {
            return int.MaxValue;
        }

        var parsed = int.TryParse(parameters.FirstOrDefault(), out var parameter);
        if (!parsed) {
            Log.M($"Foldout attribute {displayName} has invalid parameters. Expected a single integer parameter, but " +
                  $"got {parameters.Length}. Parameters: `<i>{string.Join(", ", parameters)}</i>`");
        }

        return parameter;
    }

    public static bool HasMiniAttribute(string displayName) {
        var s = displayName.ToLower();
        return s.Contains("[mini]") || s.Contains("[m]");
    }

    public static bool HasGradientAttribute(MaterialProperty property) {
        return property.type == MaterialProperty.PropType.Texture && HasGradientSubstring(property.displayName);
    }

    public static bool HasGradientSubstring(string displayName) {
        var s = displayName.ToLower();
        return s.Contains("[gradient") || s.Contains("[g]") || s.Contains("[g(");
    }

    public static bool HasCurveAttribute(MaterialProperty property) {
        return property.type == MaterialProperty.PropType.Texture && HasCurveSubstring(property.displayName);
    }

    public static bool HasCurveSubstring(string displayName) {
        var s = displayName.ToLower();
        return s.Contains("[curve") || s.Contains("[c]") || s.Contains("[c(");
    }

    public static bool HasMinMaxAttribute(MaterialProperty property) {
        return property.type == MaterialProperty.PropType.Vector && HasMinMaxSubstring(property.displayName);
    }

    public static bool HasMinMaxSubstring(string displayName) {
        var s = displayName.ToLower();
        return s.Contains("[minmax") || s.Contains("[mm");
    }

    public static bool IsPureAttribute(string displayName) {
        var s = RemoveEverythingInBrackets(displayName);
        s = Regex.Replace(s, @"[\d\(\)]", "");
        return string.IsNullOrWhiteSpace(s);
    }

    public static int NumTabs(string displayName) {
        var s = displayName.ToLower();
        // Count occurrences of "[tab]" or "[t]".
        var count = Regex.Matches(s, @"\[tab\]|\[t\]").Count;
        return count;
    }

    // Comma-separated parameters.
    private static string[] ExtractAttributeParameters(string displayName, string attribute) {
        // Example string: "[MinMax(0, 1)][Header(Hello)]". Result: "0, 1".
        var regex = new Regex($@"(?i)\[{attribute}\((?<parameters>.*?)\)\]");
        var match = regex.Match(displayName);
        return match.Success ? match.Groups["parameters"].Value.Split(',') : Array.Empty<string>();
    }

    // Everything in round parenthesis.
    private static string ExtractAttributeParameter(string displayName, string attribute) {
        // Example string: "[Line(rgb(100, 90, 80))][Space(10)]". Result: "rgb(100, 90, 80)"
        var regex = new Regex($@"(?i)\[{attribute}\((?<parameter>.*?)\)\]");
        var match = regex.Match(displayName);
        return match.Success ? match.Groups["parameter"].Value : null;
    }

    private static string RemoveEverythingInBrackets(string s) {
        s = Regex.Replace(s, @" ?\[.*?\]", string.Empty);
        s = Regex.Replace(s, @" ?\{.*?\}", string.Empty);
        // Remove leading whitespace.
        s = Regex.Replace(s, @"^\s+", string.Empty);
        return s;
    }
}
}