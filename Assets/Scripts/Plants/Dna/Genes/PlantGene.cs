using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static PlantDna;

public class PlantGene
{
    public PlantGeneCategory Category;
    public string Name;
    public GeneDna Dna;

    private MethodInfo Method;
    private object[] Parameters;
    
    public PlantGene(GeneDna dna)
    {
        try
        {
            Dna = dna;
            Category = (PlantGeneCategory)Enum.Parse(typeof(PlantGeneCategory), dna.Category);
            Name = dna.Method.Name;
            Method = Category.GetLibrary().GetMethods(BindingFlags.Static | BindingFlags.Public).FirstOrDefault(x => x.Name == dna.Method.Name);
            Parameters = GetParamters(Method, dna.Method);
            if (Method == null)
            {
                throw new Exception($"Plant Gene not recognized: { dna.Method.Name}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
    }

    public void Express(Plant plant)
    {
        Method.Invoke(plant, Parameters.Prepend(plant).ToArray());
    }

    private object[] GetParamters(MethodInfo method, Method operation)
    {
        if (!method.GetParameters().Any())
        {
            return null;
        }

        return method
            .GetParameters()
            .Where(x => x.Name.ToLower() != "plant")
            .Select(parameter =>
            {
                var value = operation.Parameters.FirstOrDefault(x => x.Name.ToLower() == parameter.Name.ToLower()).Value;
                if (string.IsNullOrEmpty(value))
                {
                    throw new Exception($"parameter {parameter.Name} was not supplied for method {method.Name}.");
                }
                var @switch = new Dictionary<Type, Func<string, object>> {
                    { typeof(int), x =>  int.Parse(x) },
                    { typeof(float), x => float.Parse(x) },
                    { typeof(string), x => x },
                };
                return @switch[parameter.ParameterType](value);
            }).ToArray();
    }
}
