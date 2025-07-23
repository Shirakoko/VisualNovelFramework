using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CharacterState
{
    public string characterId;
    public bool isVisible = true;
    public Vector2 position;
}

public class CharacterManager : MonoBehaviour
{
    [Header("角色Prefab所在的预制体路径")]
    public string characterPrefabsFolder = "Prefabs/Characters";
    [Header("所有角色父节点")]
    public Transform characterContainer;

    private Dictionary<string, GameObject> activeCharacters = new Dictionary<string, GameObject>();
    private Dictionary<string, GameObject> characterPrefabs = new Dictionary<string, GameObject>();

    private void Awake()
    {
        LoadAllCharacterPrefabs();
    }

    /** 加载所有角色预制体 */
    private void LoadAllCharacterPrefabs()
    {
        GameObject[] loadedPrefabs = Resources.LoadAll<GameObject>(characterPrefabsFolder);

        foreach (var prefab in loadedPrefabs)
        {
            string characterId = prefab.name.ToLower(); // 使用小写作为键
            if (!characterPrefabs.ContainsKey(characterId))
            {
                characterPrefabs.Add(characterId, prefab);
                Debug.Log($"Loaded character prefab: {characterId}");
            }
            else
            {
                Debug.LogWarning($"Duplicate character ID: {characterId}");
            }
        }
    }

    public void UpdateCharacters(List<CharacterState> characters)
    {
        // 清除所有现有角色
        ClearAllCharacters();
        
        // 创建和更新需要的角色
        foreach (var state in characters) {
            string characterId = state.characterId.ToLower();
            
            // 检查预制体是否存在
            if (!characterPrefabs.TryGetValue(characterId, out var prefab))
            {
                Debug.LogWarning($"Character prefab not found: {characterId}");
                continue;
            }
            
            // 创建新角色实例
            var characterObj = Instantiate(prefab, characterContainer);
            characterObj.name = characterId;
            characterObj.transform.localPosition = state.position;
            characterObj.SetActive(true);
            
            // 添加到活动角色字典
            activeCharacters[characterId] = characterObj;
        }
    }
    
    /** 清除所有角色 */
    public void ClearAllCharacters()
    {
        foreach (var character in activeCharacters.Values)
        {
            Destroy(character);
        }
        activeCharacters.Clear();
    }
}