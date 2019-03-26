using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Joint : Interactable
{
    public Plant Plant;
    public Structure Root;
    public List<Structure> Connections = new List<Structure>();

    public Renderer Selecter { get; set; }

    public void Update()
    {
        if(Root != null)transform.localPosition = new Vector3(0, 0, Root.Length);
        Selecter.enabled = Plant.IsManipulatable;
    }

    public static Joint BuildNew(Plant plant, Structure root)
    {
        var model = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        model.name = "Joint";
        var joint = model.AddComponent<Joint>();
        joint.transform.localEulerAngles = Vector3.zero;
        joint.Selecter = joint.GetComponent<Renderer>();
        joint.Selecter.material.ChangeRenderMode(MaterialExtentions.BlendMode.Transparent);
        joint.Selecter.material.color = new Color(0,0.5f,1,0.25f);
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
        var structure = Branch(existingStructure.Prefab);
        Destroy(existingStructure.gameObject);
        return structure;
    }
    public void Disconnect(Structure structure)
    {
        Connections.Remove(structure);

        if (Root == structure)
        {
            Root = null;
            transform.parent = null;
            Plant = Plant.BuildFromClipping(this);
            StartCoroutine(Fall());
        }

        if (!Connections.Any() && Root == null)
        {
            Destroy(gameObject);
        }
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

    public override bool IsInteractable(FirstPersonController player)
    {
        return Plant.IsManipulatable && 
               ( player.Tool?.Type == Tool.ToolType.BranchStretcher ||
                 player.Tool?.Type == Tool.ToolType.BranchBender ||
                 player.Material is Structure );
    }
    public override void Interact(FirstPersonController player)
    {
        if (player.Tool?.Type == Tool.ToolType.BranchStretcher)
        {
            //TODO: Adjust Length
        }
        else if (player.Tool?.Type == Tool.ToolType.BranchBender)
        {
            //TODO: Adjust Direction
            StartCoroutine(Drag(player));
        }
        else if (player.Material is Structure)
        {
            Graft(player.DropMaterial() as Structure);
        }
    }

    private IEnumerator Drag(FirstPersonController player)
    {
        player.IsCursorFreeFloating = true;
        while (Input.GetMouseButton(0))
        {
            SetPosition(player.Focus.transform.position);
            yield return new WaitForEndOfFrame();
        }
        player.IsCursorFreeFloating = false;
    }
    private IEnumerator Fall()
    {
        var rigidbody = gameObject.AddComponent<Rigidbody>();
        rigidbody.angularDrag *= 10;
        rigidbody.drag *= 5;
        yield return new WaitForSeconds(3);
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