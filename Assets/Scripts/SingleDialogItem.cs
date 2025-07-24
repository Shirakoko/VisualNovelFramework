using System;
using UnityEngine;
using UnityEngine.UI;
public class SingleDialogItem : MonoBehaviour
{
    [Header("存放对话人的文字")]
    public Text speakerText;
    [Header("存放对话内容的文字")]
    public Text contentText;

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
}