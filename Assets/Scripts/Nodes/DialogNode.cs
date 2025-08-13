using System.Collections.Generic;

[System.Serializable]
public class DialogNode : Node
{
    /** 对话列表 */
    public List<DialogLine> dialogs = new List<DialogLine>();
    /** 下一节点的nodeId */
    public string nextNodeId;

    public DialogNode()
    {
        nodeType = ENodeType.Dialog;
    }
}