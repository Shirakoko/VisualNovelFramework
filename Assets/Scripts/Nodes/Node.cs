using System.Collections.Generic;

[System.Serializable]
public class Node
{
    /** 节点Id */
    public string nodeId;
    /** 背景图Id */
    public string backgroundId;
    /** 角色列表 */
    public List<CharacterState> characters = new List<CharacterState>();
}
