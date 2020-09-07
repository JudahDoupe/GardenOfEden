using System;
using System.Collections.Generic;
using System.Linq;

public class MophologyGrowthVisitor : IPlantVisitor
{

    public void VisitPlant(Plant plant)
    {
        if (plant.Branches.Count == 0)
        {
            plant.Kill();
            return;
        }
        AddBranches(plant);

        while (_untraversedNodes.Count() > 0)
        {
            var node = _untraversedNodes.Dequeue();
            AddBranches(node);
            _didUpdate[node] = ApplyRules(node.Dna.GrowthRules.Where(x => x.IsPreOrder), node);
            _traversedNodes.Enqueue(node);
        }

        while (_traversedNodes.Count() > 0)
        {
            var node = _traversedNodes.Dequeue();
            if (node.Plant != null)
            {
                _didUpdate[node] |= ApplyRules(node.Dna.GrowthRules.Where(x => !x.IsPreOrder), node);
            }

            if (node.Plant != null && _didUpdate[node])
            {
                PlantMessageBus.NodeUpdate.Publish(node);
            }
        }
    }

    private Dictionary<Node, bool> _didUpdate = new Dictionary<Node, bool>();
    private PriorityQueue _traversedNodes = new PriorityQueue(); 
    private PriorityQueue _untraversedNodes = new PriorityQueue(); 

    private void AddBranches(Node node)
    {
        foreach(var branch in node.Branches)
        {
            _untraversedNodes.Enqueue(branch);
        }
    }

    private bool ApplyRules(IEnumerable<GrowthRule> rules, Node node)
    {
        var didUpdateNode = false;
        foreach (var rule in rules)
        {
            if (node.Plant != null
                && node.Plant.StoredEnergy > rule.EnergyCost
                && rule.ShouldApplyTo(node))
            {
                rule.ApplyTo(node);
                if (node.Plant != null)
                {
                    node.Plant.StoredEnergy -= rule.EnergyCost;
                }
                didUpdateNode = true;
            }
        }
        return didUpdateNode;
    }
}

public class PriorityQueue
{
	private List<Node> data;

	public PriorityQueue()
	{
		this.data = new List<Node>();
	}

	public void Enqueue(Node item)
	{
		data.Add(item);
		int ci = data.Count - 1; // child index; start at end
		while (ci > 0)
		{
			int pi = (ci - 1) / 2; // parent index
			if (data[ci].GrowthHormone >= data[pi].GrowthHormone)
				break; // child item is larger than (or equal) parent so we're done
			var tmp = data[ci];
			data[ci] = data[pi];
			data[pi] = tmp;
			ci = pi;
		}
	}

	public Node Dequeue()
	{
		// assumes pq is not empty; up to calling code
		int li = data.Count - 1; // last index (before removal)
		var frontItem = data[0];   // fetch the front
		data[0] = data[li];
		data.RemoveAt(li);

		--li; // last index (after removal)
		int pi = 0; // parent index. start at front of pq
		while (true)
		{
			int ci = pi * 2 + 1; // left child index of parent
			if (ci > li)
				break;  // no children so done
			int rc = ci + 1;     // right child
			if (rc <= li && data[rc].GrowthHormone < data[ci].GrowthHormone) // if there is a rc (ci + 1), and it is smaller than left child, use the rc instead
				ci = rc;
			if (data[pi].GrowthHormone <= data[ci].GrowthHormone)
				break; // parent is smaller than (or equal to) smallest child so done
			var tmp = data[pi];
			data[pi] = data[ci];
			data[ci] = tmp; // swap parent and child
			pi = ci;
		}
		return frontItem;
	}

	public Node Peek()
	{
        return data[0];
    }

	public int Count()
	{
		return data.Count;
	}
}