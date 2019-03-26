using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tool : Interactable
{
    public ToolType Type;

    public override void Interact(FirstPersonController player)
    {
        player.GrabTool(this);
    }

    public enum ToolType
    {
        Axe,
        BranchStretcher,
        BranchBender,
        BranchBeefer,
    }
}

