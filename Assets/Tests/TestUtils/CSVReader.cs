using UnityEngine;
using System.Collections.Generic;
using System.IO;

namespace Tests
{
    public class CSVReader
    {
        public static List<Color[][]> ReadTextures(string file)
        {
            var lines = File.ReadAllLines(file);

            var list = new List<Color[][]>();
            var texture = new List<Color[]>();

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    list.Add(texture.ToArray());
                    texture = new List<Color[]>();
                }
                else
                {
                    var row = new List<Color>();
                    foreach(var colorData in line.Split(','))
                    {
                        var c = new Color(1,1,1,1);
                        var i = 0;
                        foreach(var value in colorData.Trim().Split(' '))
                        {
                            c[i] = float.Parse(value);
                            i++;
                        }
                        row.Add(c);
                    }
                    texture.Add(row.ToArray());
                }
            }

            list.Add(texture.ToArray());

            return list;
        }
    }
}