using System.Collections.Generic;

public class Choice
{
    /** 选项文本 */
    public string choiceText;
    /** 下个节点的nodeId */
    public string nextNodeId;
}

[System.Serializable]
public class ChoiceNode : Node
{
    /** 问题文本 */
    public string questionText;

    /** 选项列表 */
    public List<Choice> choices = new List<Choice>();

    public ChoiceNode()
    {
        nodeType = ENodeType.Choice;
    }
}