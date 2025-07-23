using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("故事Csv文件路径")]
    [Tooltip("放在StreamingAssets路径下，一定要加后缀名")]
    public string storyFilePath = "Stories/story_config.csv";
    
    public BackgroundManager backgroundManager;
    public CharacterManager characterManager;
    public DialogManager dialogManager;
    public ChoiceManager choiceManager;

    private StoryTree currentStory;

    /** 当前节点 */
    private Node currentNode;
    /** 当前对话Id */
    private int currentDialogIndex;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        StoryInitializer initializer = new StoryInitializer();
        currentStory = initializer.InitializeStory(storyFilePath);

        if (currentStory != null)
        {
            Debug.Log("故事加载成功！");
            StartStory(currentStory);
        }
    }

    /** 开启故事 */
    public void StartStory(StoryTree story)
    {
        currentStory = story;
        currentNode = story.rootNode;
        ProcessNode(currentNode);
    }

    /** 执行节点 */
    public void ProcessNode(Node node)
    {
        currentNode = node;
        currentDialogIndex = 0;

        // 更新视觉元素
        backgroundManager.SetBackground(node.backgroundId);
        characterManager.UpdateCharacters(node.characters);

        // 显示对话或选择
        if (node is DialogNode DialogNode)
        {
            ShowCurrentDialog(DialogNode);
            choiceManager.HideChoices();
        }
        else if (node is ChoiceNode choiceNode)
        {
            dialogManager.HideDialog();
            choiceManager.ShowChoices(choiceNode);
        }
    }

    /** 显示当前对话 */
    private void ShowCurrentDialog(DialogNode node)
    {
        if (currentDialogIndex < node.dialogs.Count) {
            var line = node.dialogs[currentDialogIndex];
            dialogManager.DisplayDialog(line.speakerDisplayName, line.content);
        } else {
            NextNode();
        }
    }

    /** 下一句对话 */
    public void NextDialogLine()
    {
        if (currentNode is DialogNode dialogNode)
        {
            currentDialogIndex++;
            ShowCurrentDialog(dialogNode);
        }
    }

    /** 下一个节点 */
    public void NextNode()
    {
        if (currentNode is DialogNode dialogNode)
        {
            string nextNodeId = dialogNode.nextNodeId;
            if(nextNodeId == null) { Debug.LogWarning($"不存在下一个节点Id"); return; }
            var nextNode = currentStory.GetNodeById(nextNodeId);
            if (nextNode != null) { ProcessNode(nextNode); }
            else { Debug.LogWarning($"下一个节点为null, Id: {dialogNode.nextNodeId}"); }
        }
    }

    /** 选择选项 */
    public void SelectChoice(int choiceIndex)
    {
        if (currentNode is ChoiceNode choiceNode && choiceIndex < choiceNode.choices.Count)
        {
            string nextNodeId = choiceNode.choices[choiceIndex].nextNodeId;
            if(nextNodeId == null) { Debug.LogWarning($"不存在下一个节点Id"); return; }
            var nextNode = currentStory.GetNodeById(nextNodeId);
            if (nextNode != null) { ProcessNode(nextNode); } 
            else { Debug.LogWarning($"下一个节点为null, Id: {nextNodeId}"); }
        }
    }

    /** 跳转到指定nodeId的节点 */
    public void GoToNode(string nodeId)
    {
        var node = currentStory.GetNodeById(nodeId);
        if (node != null) { ProcessNode(node); }
        else { Debug.LogWarning($"节点为null, Id: {nodeId}"); }
    }
}
