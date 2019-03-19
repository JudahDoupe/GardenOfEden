using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Joint : MonoBehaviour, IInteractable
{
    public Plant Plant;
    public Structure Root;
    public List<Structure> Connections = new List<Structure>();

    public Renderer Selecter { get; set; }

    public void Start()
    {
        transform.localEulerAngles = Vector3.zero;
        Selecter = GetComponent<Renderer>();
    }
    public void Update()
    {
        if(Root != null)transform.localPosition = new Vector3(0, 0, Root.Length);
        Selecter.enabled = Plant.IsManipulatable;
    }

    public static Joint Build(Plant plant, Structure root)
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
        }
        else
        {
            joint.transform.parent = root.transform;
            joint.transform.localScale = Vector3.one * root.Girth;
            joint.transform.localPosition = Vector3.forward * root.Length;
        }

        return joint;
    }
    public static Joint Build(Plant plant, Structure root, JointDTO dto)
    {
        var joint = Build(plant, root);
        joint.transform.localPosition = dto.LocalPosition;
        joint.Connections = dto.Connections.Select(x => Structure.Build(plant,joint,x)).ToList();
        return joint;
    }

    public Structure Branch(GameObject prefab)
    {
        var structure = Structure.Build(Plant, this, prefab);
        Connections.Add(structure);
        return structure;
    }

    public void SetPosition(Vector3 position)
    {
        transform.position = position;

        if (Root == null)
        {
            Plant.transform.position = transform.position;
        }
        else
        {
            Root.Length = Vector3.Distance(transform.position, Root.transform.position);
            Root.transform.LookAt(transform);
            transform.rotation = Root.transform.rotation;
        }
    }

    public void Interact(FirstPersonController player)
    {
        throw new NotImplementedException();
    }
    public Vector3 InteractionPosition()
    {
        return transform.position;
    }
}

[Serializable]
public class JointDTO
{
    public StructureDTO FromStructure;
    public IEnumerable<StructureDTO> Connections;
    public Vector3 LocalPosition;

    public JointDTO(StructureDTO from, Joint joint)
    {
        FromStructure = from;
        Connections = joint.Connections.Select(x => new StructureDTO(x));
        LocalPosition = joint.transform.localPosition;
    }
}