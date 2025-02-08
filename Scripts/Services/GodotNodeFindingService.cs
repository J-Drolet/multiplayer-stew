using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class GodotNodeFindingService : Node
{
    public static List<T> FindNodes<T>(Node parent)
    {
        return FindNodes<T>(parent, new());
    } 

    private static List<T> FindNodes<T>(Node parent, List<T> nodes)
    {
        foreach(Node child in parent.GetChildren(true))
        {
            if (child is T typedNode)
            {
                nodes = nodes.Append(typedNode).ToList();
            }

            nodes = FindNodes(child, nodes);
        }

        return nodes;
    } 
}
