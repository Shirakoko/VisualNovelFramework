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
    [Header("对话框面板")]
    public GameObject dialogPanel;

    private string currentContent; // 记录当前对话内容

    private bool isTyping; // 当前是否正在打字

    private Coroutine typingCoroutine; // 打字效果协程

    void Start()
    {
        // 绑定下一句按钮的响应函数
        nextButton.onClick.AddListener(GameManager.Instance.NextDialogLine);
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

    IEnumerator TypeText(string text, float typingSpeed) {
        isTyping = true;
        dialogText.text = ""; // 先清空文本
        foreach (char c in text.ToCharArray())
        {
            dialogText.text += c; // 逐个添加字符
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }
    
    /** 隐藏对话框 */
    public void HideDialog()
    {
        dialogPanel.SetActive(false);
    }
}
