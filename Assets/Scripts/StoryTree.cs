using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StoryTree
{
    public Node rootNode;
    private Dictionary<string, Node> nodeDictionary = new Dictionary<string, Node>();

    public void AddNode(Node node)
    {
        string nodeId = node.nodeId;
        if (nodeDictionary.ContainsKey(nodeId))
        {
            Debug.LogWarning($"该节点Id[{nodeId}]对应的节点已存在");
        }
        nodeDictionary.Add(nodeId, node);
    }
    
    /** 通过nodeId获取Node */
    public Node GetNodeById(string nodeId)
    {
        if (nodeId == null){ return null; }
        return nodeDictionary.TryGetValue(nodeId, out var node) ? node : null;
    }
}