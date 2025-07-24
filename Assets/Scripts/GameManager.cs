using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("故事Csv文件路径")]
    [Tooltip("放在StreamingAssets路径下，一定要加后缀名")]
    public string storyFilePath = "Stories/story_config.csv";

    [Header("打字间隔")]
    [Tooltip("文本逐个出现的时间间隔（秒）")]
    public float typingSpeed = 0.05f;

    [Space(10)]
    public BackgroundManager backgroundManager;
    public CharacterManager characterManager;
    public DialogManager dialogManager;
    public ChoiceManager choiceManager;
    public ProcessedManager processedManager;

    private StoryTree currentStory;

    #region 调试字段
    [SerializeField]
    [Header("启用调试工具")]
    private bool showDebugPanel = true;

    private string debugTargetNodeId = "";
    private LinkedList<string> jumpHistory = new LinkedList<string>();
    private const int MAX_HISTORY = 5; // 最大历史记录数
    #endregion

    /** 当前节点 */
    private Node currentNode;
    /** 当前对话Id */
    private int currentDialogIndex;

    /** 记录已经走过的节点 */
    private List<(string Speaker, string Content)> processedDialogs = new List<(string Speaker, string Content)>();

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

    // 运行时调试
    private void OnGUI()
    {
        if (!showDebugPanel || !Application.isPlaying) return;

        GUILayout.BeginArea(new Rect(10, 10, 350, 275), GUI.skin.box);
        GUILayout.Label("<b>故事调试面板</b>", new GUIStyle(GUI.skin.label) { richText = true, fontSize = 16, alignment = TextAnchor.MiddleCenter });

        // 快速跳转
        GUILayout.Label("<color=cyan>快速跳转</color>", new GUIStyle(GUI.skin.label) { richText = true });
        GUILayout.BeginHorizontal();
        debugTargetNodeId = GUILayout.TextField(debugTargetNodeId, GUILayout.Width(200));
        // GUILayout.FlexibleSpace();
        if (GUILayout.Button("跳转", GUILayout.Width(60))) { DebugJumpToNode(debugTargetNodeId); }
        GUILayout.EndHorizontal();

        // 当前节点
        GUILayout.Label($"<color=cyan>当前节点</color> <color=yellow>{currentNode?.nodeId ?? "无"}</color>", new GUIStyle(GUI.skin.label) { richText = true });

        // 历史记录
        GUILayout.Label("<color=cyan>跳转历史</color>", new GUIStyle(GUI.skin.label){ richText = true } );
        if (jumpHistory.Count > 0)
        {
            foreach (var id in jumpHistory)
            {
                if (GUILayout.Button(id, GUI.skin.label))
                {
                    debugTargetNodeId = id;
                }
            }
        }
        GUILayout.EndArea();
    }

    /** 开启故事 */
    public void StartStory(StoryTree story)
    {
        processedManager.HideProcessedDialogs();

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
        if (currentDialogIndex < node.dialogs.Count)
        {
            var line = node.dialogs[currentDialogIndex];
            dialogManager.DisplayDialog(line.speakerDisplayName, line.content);
        }
        else
        {
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
            // 把当前节点的所有对话加入已经历的对话
            foreach(var dialog in dialogNode.dialogs) {
                processedDialogs.Add((Speaker: dialog.speakerDisplayName, Content: dialog.content));
            }

            string nextNodeId = dialogNode.nextNodeId;
            if (nextNodeId == null) { Debug.LogWarning($"不存在下一个节点Id"); return; }
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
            // 把当前节点的选项问题和玩家选择都加入已经历的对话
            processedDialogs.Add((Speaker: "面临选择", Content: choiceNode.questionText));
            processedDialogs.Add((Speaker: "你的选择", Content: choiceNode.choices[choiceIndex].choiceText));
            string nextNodeId = choiceNode.choices[choiceIndex].nextNodeId;
        
            if (nextNodeId == null) { Debug.LogWarning($"不存在下一个节点Id"); return; }
            var nextNode = currentStory.GetNodeById(nextNodeId);
            if (nextNode != null){ ProcessNode(nextNode); }
            else { Debug.LogWarning($"下一个节点为null, Id: {nextNodeId}"); }
        }
    }

    /** 打开剧情回顾 */
    public void ShowProcessedPanel()
    {
        processedManager.ShowProcessedDialogs(processedDialogs);
    }

    // 调试跳转方法
    private void DebugJumpToNode(string nodeId)
    {
        if (string.IsNullOrEmpty(nodeId)) return;

        var node = currentStory?.GetNodeById(nodeId);
        if (node != null)
        {
            ProcessNode(node);
            // 添加到历史记录
            AddToHistory(nodeId);
            Debug.Log($"调试跳转成功 -> {nodeId}");
        }
        else
        {
            Debug.LogError($"节点或故事树不存在: {nodeId}");
        }
    }

    private void AddToHistory(string nodeId)
    {
        // 如果已经存在，先移除旧记录
        if (jumpHistory.Contains(nodeId))
        {
            jumpHistory.Remove(nodeId);
        }
        
        jumpHistory.AddFirst(nodeId);
        
        // 保持最大记录数
        while (jumpHistory.Count > MAX_HISTORY)
        {
            jumpHistory.RemoveLast();
        }
    }
}
