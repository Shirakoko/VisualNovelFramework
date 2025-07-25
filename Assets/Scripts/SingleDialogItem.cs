using System;
using UnityEngine;
using UnityEngine.UI;
public class SingleDialogItem : MonoBehaviour
{
    [Header("存放对话人的文字")]
    public Text speakerText;
    [Header("存放对话内容的文字")]
    public Text contentText;

    private void Start()
    {
        AdjustHeight();   
    }

    public void SetSpeaker(string speaker)
    {
        if (speakerText != null)
        {
            speakerText.text = speaker;
        }
    }

    public void SetContent(string content)
    {
        if (contentText != null)
        {
            contentText.text = content;
        }
    }

    public void AdjustHeight()
    {
        // 获取两个子控件的实际高度
        float speakerHeight = speakerText.preferredHeight;
        float contentHeight = contentText.preferredHeight;
        
        // 取最大高度作为自身高度
        float maxHeight = Mathf.Max(speakerHeight, contentHeight);
        
        // 设置自身高度
        RectTransform rt = GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(rt.sizeDelta.x, maxHeight);
    }
}