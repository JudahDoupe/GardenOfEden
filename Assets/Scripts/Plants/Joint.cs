using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Joint : MonoBehaviour
{
    public Plant Plant;
    public Structure Root;
    public List<Structure> Connections = new List<Structure>();

    public static Joint Build(Structure root, Plant plant)
    {
        var model = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        model.name = "Joint";
        var joint = model.AddComponent<Joint>();
        joint.Root = root;
        joint.Plant = plant;

        if (root == null)
        {
            joint.transform.parent = plant.transform;
            joint.transform.LookAt(Vector3.up);
            return joint;
        }

        joint.transform.parent = root.transform;
        joint.transform.localScale = Vector3.one * root.Girth;
        joint.transform.localPosition = Vector3.forward * root.Length;
        return joint;
    }

    public Structure Branch(GameObject prefab)
    {
        var structure = Structure.Build(this, prefab, Plant);
        Connections.Add(structure);
        return structure;
    }

    public void Click(Vector3 position)
    {
        if (Plant.GetStructure() == null) return;
        Branch(Plant.GetStructure());
        Plant.ClearStructure();
    }
    public void Drag(Vector3 newPos)
    {
        if (Plant.GetStructure() == null)
        {
            SetPosition(newPos);
        }
    }

    public void SetPosition(Vector3 position)
    {
        transform.position = position;

        if (Root == null) return;
        Root.transform.LookAt(transform);
        transform.rotation = Root.transform.rotation;
        Root.Model.transform.localScale = new Vector3(
            Root.Model.transform.localScale.x,
            Root.Model.transform.localScale.y,
            Vector3.Distance(transform.position, Root.transform.position));
    }
    public void ResetPosition()
    {
        transform.localPosition = new Vector3(0, 0, Root.Model.transform.localScale.z);
    }
}
