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

    void Start()
    {
        // 绑定下一句按钮的响应函数
        nextButton.onClick.AddListener(GameManager.Instance.NextDialogLine);
    }

    /** 显示对话框 */
    public void DisplayDialog(string speakerName, string content)
    {
        dialogPanel.SetActive(true);
        speakerNameText.text = speakerName;
        dialogText.text = content;
        //TODO 文字打字机动效待做
    }
    
    /** 隐藏对话框 */
    public void HideDialog()
    {
        dialogPanel.SetActive(false);
    }
}
