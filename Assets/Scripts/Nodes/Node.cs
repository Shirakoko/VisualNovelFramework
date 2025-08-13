using System.Collections.Generic;

public enum ENodeType
{
    Dialog,     // 对话节点
    Choice,     // 选择节点
    // Action,   // 执行某些操作
    // End,      // 结束节点
}

[System.Serializable]
public class Node
{
    /** 节点类型 */
    public ENodeType nodeType;

    /** 节点Id */
    public string nodeId;
    /** 背景图Id */
    public string backgroundId;
    /** 角色列表 */
    public List<CharacterState> characters = new List<CharacterState>();
}
