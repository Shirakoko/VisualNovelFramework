using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public struct DialogLine
{
    // public string speakerId; // 说话人ID
    public string speakerDisplayName; // 显示的名字（可能和ID不同）
    public string content; // 对话内容
}

public class DialogManager : MonoBehaviour
{
    [Header("对话人文本")]
    public Text speakerNameText;

    [Header("对话内容文本")]
    public Text dialogText;

    [Header("下一句按钮")]
    public Button nextButton;

    [Header("自动播放按钮")]
    public Button autoButton;

    private Text autoButtonText;

    [Header("打开自动播放延迟跳转时间")]
    public float delayStartAutoPlay = 0.5f;

    [Header("对话框面板")]
    public GameObject dialogPanel;

    private bool isAutoPlay = false; // 是否自动播放

    private string currentContent; // 记录当前对话内容

    private bool isTyping; // 当前是否正在打字

    private Coroutine typingCoroutine; // 打字效果协程

    private void Awake()
    {
        autoButtonText = autoButton.GetComponentInChildren<Text>();
    }

    private void Start()
    {
        // 绑定下一句按钮的响应函数
        nextButton.onClick.AddListener(GameManager.Instance.NextDialogLine);
        autoButton.onClick.AddListener(ToggleAutoPlay);
        autoButtonText.text = isAutoPlay ? "Auto(On)" : "Auto(Off)";
    }

    private void ToggleAutoPlay()
    {
        isAutoPlay = !isAutoPlay;
        autoButtonText.text = isAutoPlay ? "Auto(On)" : "Auto(Off)";

         // 如果开启自动播放且当前没有在打字，需要调用下一句
        if (isAutoPlay && !isTyping)
        {
            StartCoroutine(DelayedAutoAdvance());
        }
    }

    IEnumerator DelayedAutoAdvance()
    {
        yield return new WaitForSeconds(delayStartAutoPlay);
        GameManager.Instance.NextDialogLine();
    }

    /** 显示对话框 */
    public void DisplayDialog(string speakerName, string content)
    {
        // 先停止原来正在进行的打字效果
        if (isTyping)
        {
            StopCoroutine(typingCoroutine);
            dialogText.text = currentContent;
            isTyping = false;
        }

        dialogPanel.SetActive(true);
        speakerNameText.text = speakerName;
        currentContent = content;

        // 开始打字
        typingCoroutine = StartCoroutine(TypeText(content, GameManager.Instance.typingSpeed));
    }

    IEnumerator TypeText(string text, float typingSpeed)
    {
        isTyping = true;
        dialogText.text = ""; // 先清空文本
        foreach (char c in text.ToCharArray())
        {
            dialogText.text += c; // 逐个添加字符
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;

        // 自动播放模拟点击Next按钮
        if (isAutoPlay)
        {
            yield return null;
            nextButton.onClick.Invoke();
        }
    }
    
    /** 隐藏对话框 */
    public void HideDialog()
    {
        dialogPanel.SetActive(false);
    }
}
