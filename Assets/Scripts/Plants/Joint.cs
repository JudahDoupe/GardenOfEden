using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Joint : Interactable
{
    public Plant Plant;
    public Structure Base;
    public List<Structure> Connections = new List<Structure>();

    public Renderer Selecter { get; set; }

    public void Update()
    {
        if (Base != null)
        {
            transform.localPosition = new Vector3(0, 0, Base.Length);
        }
        Selecter.enabled = Plant.IsManipulatable;
    }

    public static Joint BuildNew(Plant plant, Structure root)
    {
        var model = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        model.name = "Joint";
        var joint = model.AddComponent<Joint>();
        joint.GetComponent<SphereCollider>().isTrigger = true;
        joint.Selecter = joint.GetComponent<Renderer>();
        joint.Selecter.material.ChangeRenderMode(MaterialExtentions.BlendMode.Transparent);
        joint.Selecter.material.color = new Color(0,0.5f,1,0.25f);
        joint.Base = root;
        joint.Plant = plant;

        if (root == null)
        {
            joint.transform.parent = plant.transform;
            joint.transform.LookAt(Vector3.up);
        }
        else
        {
            joint.transform.parent = root.transform;
            joint.transform.localScale = joint.Plant.IsManipulatable ? Vector3.one : Vector3.one * root.Girth;
            joint.transform.localPosition = Vector3.forward * root.Length;
            joint.transform.localEulerAngles = Vector3.zero;
        }

        return joint;
    }
    public static Joint BuildDto(Plant plant, Structure root, JointDTO dto)
    {
        var joint = BuildNew(plant, root);
        joint.transform.localPosition = dto.LocalPosition;
        joint.Connections = dto.Connections.Select(x => Structure.BuildDto(plant,joint,x)).ToList();
        return joint;
    }

    public Structure Branch(GameObject prefab)
    {
        var structure = Structure.BuildPrefab(Plant, this, prefab);
        Connections.Add(structure);
        return structure;
    }
    public Structure Graft(Structure existingStructure)
    {
        var newStructure = Branch(existingStructure.Prefab);
        Destroy(existingStructure.gameObject);
        return newStructure;
    }
    public void Disconnect(Structure structure)
    {
        Connections.Remove(structure);

        if (Base == structure)
        {
            BuildNew(Plant, structure);
            Base = null;
            Plant = Plant.BuildFromClipping(this); //Maybe we dont want to do this???
            transform.parent = null;
            StartCoroutine(Fall());
        }

        if (!Connections.Any() && Base == null)
        {
            Destroy(gameObject);
        }    

        structure.TryBecomingItem();
        Connections.ForEach(x => x.TryBecomingItem());
    }

    public void SetPosition(Vector3 position)
    {
        transform.position = position;

        if (Base == null)
        {
            Plant.transform.position = transform.position;
        }
        else
        {
            Base.Length = Vector3.Distance(transform.position, Base.transform.position);
            Base.transform.LookAt(transform);
            transform.rotation = Base.transform.rotation;
        }
    }
    public override bool IsInteractable(FirstPersonController player)
    {
        return false;
    }

    private IEnumerator Fall()
    {
        var rigidbody = gameObject.AddComponent<Rigidbody>();
        rigidbody.angularDrag *= 10;
        rigidbody.drag *= 5;
        yield return new WaitForSeconds(1);
        while (rigidbody.velocity.magnitude > 0.0001f)
        {
            yield return new WaitForEndOfFrame();
        }
        Destroy(rigidbody);
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