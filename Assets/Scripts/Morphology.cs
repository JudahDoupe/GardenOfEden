using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Morphology
{
    private readonly Dictionary<Node, List<Connection>> _connections = new Dictionary<Node, List<Connection>>();

    public Node GetStartingNode()
    {
        return _connections.Keys.FirstOrDefault();
    }
    public List<Connection> GetNodeConnections(Node node)
    {
        return _connections[node];
    }

    public Node AddNode(GameObject structure)
    {
        var node = new Node
        {
            LocalScale = new Vector3(1,1,1),
            Structure = structure
        };
        _connections.Add(node, new List<Connection>());
        return node;
    }
    public Connection AddConnection(Node from, Node to)
    {
        var connection = new Connection
        {
            Direction = Vector3.zero,
            ToNode = to
        };
        _connections[from].Add(connection);
        return connection;
    }

}

[Serializable]
public class Node
{
    public Vector3 LocalScale;
    public GameObject Structure;
    public float TimeToGrow;
}
[Serializable]
public class Connection
{
    public Vector3 Direction;
    public Node ToNode;
}