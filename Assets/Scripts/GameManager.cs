using System.Collections.Generic;
using System.Linq;
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

    [Header("黑幕")]
    public ScreenFader screenFader;
    [Header("黑幕淡入淡出时间")]
    [SerializeField]
    private float fadeTime = 0.5f;

    [Space(10)]
    public BackgroundManager backgroundManager;
    public CharacterManager characterManager;
    public DialogManager dialogManager;
    public ChoiceManager choiceManager;
    public ProcessedManager processedManager;
    public SaveManager saveManager;

    private StoryTree currentStory;
    public StoryTree CurrentStory => currentStory;

    #region 调试字段
    [SerializeField]
    [Header("启用调试工具")]
    private bool showDebugPanel = false;

    private string debugTargetNodeId = "";
    private LinkedList<string> jumpHistory = new LinkedList<string>();
    private const int MAX_HISTORY = 5; // 最大历史记录数
    #endregion

    /** 当前节点 */
    private Node currentNode;
    public Node CurrentNode => currentNode;
    /** 当前对话Id */
    [HideInInspector]
    private int currentDialogIndex;
    public int CurrentDialogIndex => currentDialogIndex;

    /** 记录已经走过的对话内容 */
    [HideInInspector]
    public List<(string Speaker, string Content)> processedDialogs = new List<(string Speaker, string Content)>();

    // 记录所有走过的节点（按顺序）
    public List<Node> processedNodes = new List<Node>();

    // 记录选择节点的索引（栈顶是最近的选择节点）
    public Stack<int> processedChoiceNodeIndices = new Stack<int>();

    // 记录对话节点的索引（栈顶是最近的对话节点）
    public Stack<int> processedDialogNodeIndices = new Stack<int>();

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
        processedManager.HideProcessedDialogs();
        saveManager.HideSavePanel();

        currentStory = story;
        currentNode = story.rootNode;
        ProcessNode(currentNode);
    }

    /** 执行节点 */
    private void ProcessNode(Node node, int dialogIndex = 0, bool isChoiceNodeRetreat = false)
    {
        currentNode = node;
        currentDialogIndex = dialogIndex;

        // 更新视觉元素
        backgroundManager.SetBackground(node.backgroundId);
        characterManager.UpdateCharacters(node.characters);

        // 显示对话或选择
        if (node is DialogNode dialogNode)
        {
            ShowCurrentDialog(dialogNode);
            choiceManager.HideChoices();
        }
        else if (node is ChoiceNode choiceNode)
        {

            choiceManager.ShowChoices(choiceNode);
    
            // 对话框显示choiceNode之前的dialogNode的内容
            if (isChoiceNodeRetreat && processedDialogNodeIndices.Count > 0) {
                var lastDialogNodeIndex = processedDialogNodeIndices.Peek();
                if (lastDialogNodeIndex < processedNodes.Count)
                {
                    var lastDialogNode = processedNodes[lastDialogNodeIndex] as DialogNode;
                    var lastLine = lastDialogNode.dialogs.Last();
                    dialogManager.DisplayDialog(lastLine.speakerDisplayName, lastLine.content);
                }
            }
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
            if (node.nextNodeId == null) { Debug.LogWarning($"当前对话节点{currentNode.nodeId} 不存在下一个节点Id"); return; }
            NextNode(node.nextNodeId);
        }
    }

    /** 下一句对话 */
    public void NextDialogLine()
    {
        if (currentNode is DialogNode dialogNode)
        {
            if (currentDialogIndex >= dialogNode.dialogs.Count)
            {
                Debug.LogWarning($"当前已是最后一句对话, 节点Id: {dialogNode.nodeId}");
                return;
            }
            // 把前一句对话加入已经历的对话
            var line = dialogNode.dialogs[currentDialogIndex];
            processedDialogs.Add((Speaker: line.speakerDisplayName, Content: line.content));

            currentDialogIndex++;
            ShowCurrentDialog(dialogNode);
        }
    }


    /** 下一个节点 */
    private void NextNode(string nextNodeId)
    {
        processedNodes.Add(currentNode);

        if (currentNode.nodeType == ENodeType.Dialog)
        {
            processedDialogNodeIndices.Push(processedNodes.Count - 1);
        }
        else if (currentNode.nodeType == ENodeType.Choice)
        {
            processedChoiceNodeIndices.Push(processedNodes.Count - 1);
        }
    
        var nextNode = currentStory.GetNodeById(nextNodeId);
        if (nextNode != null) { ProcessNode(nextNode); }
        else { Debug.LogWarning($"下一个节点为null, 节点Id: {nextNodeId}"); }
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
            if (nextNodeId == null) { Debug.LogWarning($"当前选择节点{currentNode.nodeId} 不存在下一个节点Id"); return; }
            NextNode(nextNodeId);
        }
    }

    /** 跳转到上一个选择节点 */
    public void BackToLastChoiceNode()
    {
        if (processedChoiceNodeIndices.Count > 0)
        {
            int targetChoiceIndex = processedChoiceNodeIndices.Pop();

            // 清理processedNodes，截断到目标节点
            processedNodes = processedNodes.GetRange(0, targetChoiceIndex + 1);

            // 移除所有在目标节点之后的对话索引，比如对话：0,1,3,5,6；选择：2,4，回退到4要移除5、6
            while (processedDialogNodeIndices.Count > 0 &&
            processedDialogNodeIndices.Peek() > targetChoiceIndex)
            {
                processedDialogNodeIndices.Pop();
            }

            screenFader.gameObject.SetActive(true);
            StartCoroutine(screenFader.FadeOutAndIn(() =>
            {
                Node targetNode = processedNodes[targetChoiceIndex]; // 等效于.Last()
                ProcessNode(targetNode, 0, true); // isChoiceNodeRetreat = true
            }, this.fadeTime));
        }
        else
        {
            Debug.LogWarning($"当前进度不存在上一个选择节点, 当前节点: {currentNode.nodeId}");
        }
    }

    /** 加载指定存档节点 */
    public void LoadSaveData(SaveData data)
    {
        processedDialogs = DialogDataConverter.ToTuples(data.processedDialogs);

        var node = currentStory.GetNodeById(data.nodeId);
        if (node != null)
        {
            screenFader.gameObject.SetActive(true);
            StartCoroutine(screenFader.FadeOutAndIn(() =>
            {
                ProcessNode(node, data.dialogIndex, true); // isChoiceNodeRetreat = true
            }, this.fadeTime));
        }
    }

    /** 打开剧情回顾 */
    public void ShowProcessedPanel()
    {
        processedManager.ShowProcessedDialogs(processedDialogs);
    }

    #region 调试工具相关
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
        GUILayout.Label("<color=cyan>跳转历史</color>", new GUIStyle(GUI.skin.label) { richText = true });
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

    /** 根据背景图Id得到背景图Sprite资源 */
    public Sprite GetBGSpriteById(string backgroundId)
    {
        return backgroundManager.GetBGSpriteById(backgroundId);
    }

    // 调试跳转方法
    private void DebugJumpToNode(string nodeId)
    {
        if (string.IsNullOrEmpty(nodeId)) return;

        var node = currentStory?.GetNodeById(nodeId);
        if (node != null)
        {
            ProcessNode(node, 0, true);
            // 添加到历史记录
            AddToHistory(nodeId);
            Debug.Log($"调试跳转成功 -> {nodeId}");
        }
        else
        {
            Debug.LogError($"节点或故事树不存在: {nodeId}");
        }
    }

    // 添加跳转历史记录
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
    #endregion
}
