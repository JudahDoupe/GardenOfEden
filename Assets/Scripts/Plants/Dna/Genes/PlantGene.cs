using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class PlantGene : IDataBaseObject<PlantGeneDto>
{
    public PlantGeneCategory Category;
    public string Name;

    private PlantGeneDto Dto;
    private MethodInfo Method;
    private object[] Parameters;
    
    public PlantGene(PlantGeneDto dto)
    {
        try
        {
            Dto = dto;
            Category = (PlantGeneCategory)Enum.Parse(typeof(PlantGeneCategory), dto.Category);
            Name = dto.Method.Name;
            var library = Category.GetLibrary().GetMethods(BindingFlags.Static | BindingFlags.Public);
            Method = library.FirstOrDefault(x => x.Name == dto.Method.Name);
            if (Method == null)
            {
                throw new Exception($"Plant Gene not recognized: { dto.Method.Name}");
            }
            Parameters = GetParamters(Method, dto.Method);
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

    private object[] GetParamters(MethodInfo method, MethodDto operation)
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

    public PlantGeneDto ToDto()
    {
        return Dto;
    }
}

public class PlantGeneDto
{
    public string Category { get; set; }
    public MethodDto Method { get; set; }
}